import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Label } from '../ui/label';
import { Switch } from '../ui/switch';
import { Bell } from 'lucide-react';

interface NotificationSettingsProps {
  notificationSettings: Record<string, boolean>;
  onNotificationSettingChange: (setting: string, value: boolean) => void;
}

export function NotificationSettings({ 
  notificationSettings, 
  onNotificationSettingChange 
}: NotificationSettingsProps) {
  const notificationOptions = [
    {
      key: 'lowStockAlerts',
      label: 'Low Stock Alerts',
      description: 'Get notified when inventory is running low'
    },
    {
      key: 'dailyReports',
      label: 'Daily Reports',
      description: 'Receive daily sales and performance summaries'
    },
    {
      key: 'salesTargets',
      label: 'Sales Target Notifications',
      description: 'Alerts for sales target achievements and milestones'
    },
    {
      key: 'systemMaintenance',
      label: 'System Maintenance',
      description: 'Notifications about system updates and maintenance'
    },
    {
      key: 'emailNotifications',
      label: 'Email Notifications',
      description: 'Send notifications via email'
    },
    {
      key: 'smsNotifications',
      label: 'SMS Notifications',
      description: 'Send critical alerts via SMS'
    },
  ];

  return (
    <div className="space-y-6">
      <Card className="pos-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5 text-[#D4AF37]" />
            Notification Preferences
          </CardTitle>
          <CardDescription>
            Configure when and how you receive notifications
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-6">
            {notificationOptions.map((option) => (
              <div key={option.key} className="flex items-center justify-between">
                <div className="space-y-1">
                  <Label htmlFor={option.key}>{option.label}</Label>
                  <p className="text-sm text-muted-foreground">
                    {option.description}
                  </p>
                </div>
                <Switch
                  id={option.key}
                  checked={notificationSettings[option.key]}
                  onCheckedChange={(checked) => onNotificationSettingChange(option.key, checked)}
                />
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}