export type ResourceType = 'MeetingRoom' | 'Vehicle' | 'Equipment' | 'LabSlot';
export type BookingStatus = 'Confirmed' | 'Cancelled';
export type UserRole = 'Staff' | 'Administrator';

export interface Resource {
  id: string;
  name: string;
  type: ResourceType;
  capacity: number;
  location: string;
  isActive: boolean;
}

export interface Booking {
  id: string;
  resourceId: string;
  resourceName: string;
  requesterId: string;
  requesterName: string;
  startsAt: string;
  endsAt: string;
  purpose: string;
  status: BookingStatus;
  createdAt: string;
}

export interface BookingAudit {
  id: number;
  fromStatus: BookingStatus | null;
  toStatus: BookingStatus;
  changedBy: string;
  changedAt: string;
  note: string;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface CreateBooking {
  resourceId: string;
  requesterId: string;
  startsAt: string;
  endsAt: string;
  purpose: string;
}
