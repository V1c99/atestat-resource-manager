import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { Booking } from '../models';

@Component({
  selector: 'app-week-schedule',
  imports: [DatePipe],
  templateUrl: './week-schedule.html',
  styleUrl: './week-schedule.css'
})
export class WeekSchedule {
  readonly bookings = input.required<Booking[]>();
  readonly weekStart = input.required<Date>();
  readonly cancelled = output<Booking>();

  readonly days = computed(() => {
    const start = this.weekStart();
    return [0, 1, 2, 3, 4, 5, 6].map(offset => {
      const day = new Date(start);
      day.setDate(start.getDate() + offset);
      return day;
    });
  });

  bookingsOn(day: Date): Booking[] {
    return this.bookings().filter(booking => {
      const start = new Date(booking.startsAt);
      return start.getFullYear() === day.getFullYear()
        && start.getMonth() === day.getMonth()
        && start.getDate() === day.getDate();
    });
  }
}
