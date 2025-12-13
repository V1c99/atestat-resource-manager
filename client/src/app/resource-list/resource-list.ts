import { Component, input, output } from '@angular/core';
import { Resource } from '../models';

@Component({
  selector: 'app-resource-list',
  imports: [],
  templateUrl: './resource-list.html',
  styleUrl: './resource-list.css'
})
export class ResourceList {
  readonly resources = input.required<Resource[]>();
  readonly selectedId = input<string | null>(null);
  readonly picked = output<Resource>();
}
