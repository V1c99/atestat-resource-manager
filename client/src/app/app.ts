import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Api } from './api';
import { BookingForm } from './booking-form/booking-form';
import { Booking, BookingAudit, Resource, User } from './models';
import { ResourceList } from './resource-list/resource-list';
import { WeekSchedule } from './week-schedule/week-schedule';

function mondayOf(date: Date): Date {
  const monday = new Date(date);
  const weekday = (monday.getDay() + 6) % 7;
  monday.setDate(monday.getDate() - weekday);
  monday.setHours(0, 0, 0, 0);
  return monday;
}

@Component({
  selector: 'app-root',
  imports: [DatePipe, ResourceList, WeekSchedule, BookingForm],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly api = inject(Api);

  readonly resources = signal<Resource[]>([]);
  readonly users = signal<User[]>([]);
  readonly bookings = signal<Booking[]>([]);
  readonly selected = signal<Resource | null>(null);
  readonly weekStart = signal(mondayOf(new Date()));
  readonly auditFor = signal<Booking | null>(null);
  readonly audit = signal<BookingAudit[]>([]);

  private weekRequest = 0;

  ngOnInit() {
    this.api.users().subscribe(users => this.users.set(users));
    this.api.resources().subscribe(resources => {
      this.resources.set(resources);
      if (resources.length > 0) {
        this.pick(resources[0]);
      }
    });
  }

  pick(resource: Resource) {
    this.selected.set(resource);
    this.auditFor.set(null);
    this.loadWeek();
  }

  moveWeek(weeks: number) {
    const next = new Date(this.weekStart());
    next.setDate(next.getDate() + weeks * 7);
    this.weekStart.set(next);
    this.loadWeek();
  }

  loadWeek() {
    const resource = this.selected();
    if (resource === null) {
      return;
    }

    const from = this.weekStart();
    const to = new Date(from);
    to.setDate(to.getDate() + 7);

    // Holding Previous down fires a request per click and they do not come back in order.
    // Only the answer to the last one is allowed to win.
    const request = ++this.weekRequest;
    this.api.schedule(resource.id, from, to).subscribe(bookings => {
      if (request === this.weekRequest) {
        this.bookings.set(bookings);
      }
    });
  }

  // TODO: prompt() is ugly. A small dialog would be better but I ran out of weekend.
  cancel(booking: Booking) {
    const reason = window.prompt('Why is this booking cancelled?');
    if (reason === null || reason.trim() === '') {
      return;
    }

    this.api.cancelBooking(booking.id, booking.requesterId, reason).subscribe(() => this.loadWeek());
  }

  showAudit(booking: Booking) {
    this.auditFor.set(booking);
    this.api.audit(booking.id).subscribe(rows => this.audit.set(rows));
  }

  closeAudit() {
    this.auditFor.set(null);
  }
}
