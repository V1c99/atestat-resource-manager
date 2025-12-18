import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Booking, BookingAudit, CreateBooking, Resource, User } from './models';

@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBase;

  resources() {
    return this.http.get<Resource[]>(`${this.base}/resources`);
  }

  users() {
    return this.http.get<User[]>(`${this.base}/users`);
  }

  schedule(resourceId: string, from: Date, to: Date) {
    const params = { from: from.toISOString(), to: to.toISOString() };
    return this.http.get<Booking[]>(`${this.base}/resources/${resourceId}/schedule`, { params });
  }

  createBooking(booking: CreateBooking) {
    return this.http.post<Booking>(`${this.base}/bookings`, booking);
  }

  cancelBooking(id: string, cancelledBy: string, reason: string) {
    return this.http.post<Booking>(`${this.base}/bookings/${id}/cancel`, { cancelledBy, reason });
  }

  audit(bookingId: string) {
    return this.http.get<BookingAudit[]>(`${this.base}/bookings/${bookingId}/audit`);
  }
}
