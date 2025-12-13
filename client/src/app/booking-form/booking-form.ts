import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';
import { Resource, User } from '../models';

@Component({
  selector: 'app-booking-form',
  imports: [FormsModule],
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

  readonly saving = signal(false);

  submit() {
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
      error: () => {
        this.saving.set(false);
      }
    });
  }
}
