import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Booking, CreateBooking, Resource, User } from './models';

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

  createBooking(booking: CreateBooking) {
    return this.http.post<Booking>(`${this.base}/bookings`, booking);
  }
}
