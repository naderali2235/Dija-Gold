// Default gold rates - now fetched from API instead of hardcoded values
export const DEFAULT_GOLD_RATES = {
  '24k': '0', // Will be populated from API
  '22k': '0', // Will be populated from API
  '21k': '0', // Will be populated from API
  '18k': '0', // Will be populated from API
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

export const DEFAULT_OWNERSHIP_SETTINGS = {
  lowOwnershipThreshold: '50',
  highOwnershipThreshold: '80',
  criticalOwnershipThreshold: '30',
  enableOwnershipValidation: true,
  enableOwnershipAlerts: true,
  enablePaymentTracking: true,
  autoGenerateAlerts: true,
  alertSeverityLevels: {
    low: '20',
    medium: '50',
    high: '80',
  },
  validationRules: {
    preventSaleBelowThreshold: true,
    requirePaymentConfirmation: true,
    enableTransferValidation: true,
    enableInventoryValidation: true,
  },
  notificationSettings: {
    emailAlerts: true,
    dashboardAlerts: true,
    reportAlerts: true,
    lowOwnershipAlerts: true,
    outstandingPaymentAlerts: true,
  },
};

export const OWNERSHIP_SEVERITY_OPTIONS = [
  { value: 'low', label: 'Low' },
  { value: 'medium', label: 'Medium' },
  { value: 'high', label: 'High' },
  { value: 'critical', label: 'Critical' },
];

export const VALIDATION_RULE_OPTIONS = [
  { value: 'preventSaleBelowThreshold', label: 'Prevent sales below ownership threshold' },
  { value: 'requirePaymentConfirmation', label: 'Require payment confirmation' },
  { value: 'enableTransferValidation', label: 'Validate ownership during transfers' },
  { value: 'enableInventoryValidation', label: 'Validate ownership during inventory operations' },
];