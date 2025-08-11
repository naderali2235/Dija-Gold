import { Component } from '@angular/core';
import { NotificationService, Notification } from '@core/services/notification.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent {
  notifications$ = this.notificationService.notifications$;

  constructor(private notificationService: NotificationService) {}

  trackById(_: number, item: Notification) { return item.id; }

  dismiss(id: string) { this.notificationService.dismiss(id); }
}


