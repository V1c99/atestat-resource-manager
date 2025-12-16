import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';
import { Booking, Resource, User } from '../models';

@Component({
  selector: 'app-booking-form',
  imports: [DatePipe, FormsModule],
  templateUrl: './booking-form.html',
  styleUrl: './booking-form.css'
})
export class BookingForm {
  readonly resource = input.required<Resource>();
  readonly users = input.required<User[]>();
  readonly saved = output<void>();

  private readonly api = inject(Api);

  date = new Date().toISOString().slice(0, 10);
  startTime = '10:00';
  endTime = '11:00';
  requesterId = '';
  purpose = '';

  readonly error = signal('');
  readonly clash = signal<Booking | null>(null);
  readonly saving = signal(false);

  submit() {
    this.error.set('');
    this.clash.set(null);
    this.saving.set(true);

    const startsAt = new Date(`${this.date}T${this.startTime}:00`).toISOString();
    const endsAt = new Date(`${this.date}T${this.endTime}:00`).toISOString();

    this.api.createBooking({
      resourceId: this.resource().id,
      requesterId: this.requesterId,
      startsAt,
      endsAt,
      purpose: this.purpose
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.purpose = '';
        this.saved.emit();
      },
      error: (response: HttpErrorResponse) => {
        this.saving.set(false);
        this.error.set(this.describe(response));
        this.clash.set(response.error?.clashesWith ?? null);
      }
    });
  }

  private describe(response: HttpErrorResponse): string {
    if (response.status === 409) {
      return response.error?.message ?? 'That slot is already taken.';
    }

    if (response.status === 400 && response.error?.errors) {
      const messages = Object.values(response.error.errors) as string[][];
      return messages.flat().join(' ');
    }

    return `The server answered ${response.status}.`;
  }
}
