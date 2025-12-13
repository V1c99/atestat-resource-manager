import { Component, inject, OnInit, signal } from '@angular/core';
import { Api } from './api';
import { BookingForm } from './booking-form/booking-form';
import { Resource, User } from './models';
import { ResourceList } from './resource-list/resource-list';

@Component({
  selector: 'app-root',
  imports: [ResourceList, BookingForm],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly api = inject(Api);

  readonly resources = signal<Resource[]>([]);
  readonly users = signal<User[]>([]);
  readonly selected = signal<Resource | null>(null);

  ngOnInit() {
    this.api.users().subscribe(users => this.users.set(users));
    this.api.resources().subscribe(resources => {
      this.resources.set(resources);
      if (resources.length > 0) {
        this.selected.set(resources[0]);
      }
    });
  }

  pick(resource: Resource) {
    this.selected.set(resource);
  }
}
