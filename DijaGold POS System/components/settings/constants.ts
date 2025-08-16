import { GOLD_RATES_EGP } from '../utils/currency';

export const DEFAULT_GOLD_RATES = {
  '24k': GOLD_RATES_EGP['24k'].toString(),
  '22k': GOLD_RATES_EGP['22k'].toString(),
  '18k': GOLD_RATES_EGP['18k'].toString(),
  '14k': GOLD_RATES_EGP['14k'].toString(),
};

export const DEFAULT_TAX_SETTINGS = {
  gst: '14',
  makingChargesMin: '300',
  makingChargesMax: '2000',
  loyaltyPointsRate: '0.1',
};

export const DEFAULT_SYSTEM_SETTINGS = {
  companyName: 'DijaGold Jewelry',
  address: '123 Khan El-Khalili, Cairo, Egypt',
  phone: '+20 100 123 4567',
  email: 'info@dijapos.com',
  currency: 'EGP',
  timezone: 'Africa/Cairo',
  language: 'en',
  lowStockThreshold: '5',
  autoBackup: true,
  receiptFooter: 'Thank you for shopping with DijaGold!',
};

export const DEFAULT_SECURITY_SETTINGS = {
  passwordExpiry: '90',
  maxLoginAttempts: '3',
  sessionTimeout: '30',
  requireStrongPassword: true,
  enableTwoFactor: false,
  auditLogging: true,
};

export const DEFAULT_NOTIFICATION_SETTINGS = {
  lowStockAlerts: true,
  dailyReports: true,
  salesTargets: false,
  systemMaintenance: true,
  emailNotifications: true,
  smsNotifications: false,
};

export const DEFAULT_HARDWARE_SETTINGS = {
  receiptPrinter: 'EPSON-TM-T20',
  cashDrawer: 'Connected',
  barcodeScanner: 'Honeywell-1900',
  scale: 'Ohaus-Navigator',
  networkStatus: 'Connected',
  backupLocation: 'Local & Cloud',
};

export const CURRENCY_OPTIONS = [
  { value: 'EGP', label: 'Egyptian Pound (EGP)' },
  { value: 'USD', label: 'US Dollar (USD)' },
  { value: 'EUR', label: 'Euro (EUR)' },
];

export const TIMEZONE_OPTIONS = [
  { value: 'Africa/Cairo', label: 'Cairo (UTC+2)' },
  { value: 'Europe/London', label: 'London (UTC+0)' },
  { value: 'America/New_York', label: 'New York (UTC-5)' },
];

export const LANGUAGE_OPTIONS = [
  { value: 'en', label: 'English' },
  { value: 'ar', label: 'Arabic' },
];

export const PRINTER_OPTIONS = [
  { value: 'EPSON-TM-T20', label: 'EPSON TM-T20' },
  { value: 'STAR-TSP143', label: 'STAR TSP143' },
  { value: 'CITIZEN-CT-S310', label: 'CITIZEN CT-S310' },
];