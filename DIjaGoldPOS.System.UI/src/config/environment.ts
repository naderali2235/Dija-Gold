/**
 * Environment configuration for DijaGold POS System
 */

// API Configuration
export const API_CONFIG = {
  BASE_URL: process.env.REACT_APP_API_URL || 'https://localhost:5001/api',
  TIMEOUT: 30000, // 30 seconds
  RETRY_ATTEMPTS: 3,
};

// Application Configuration
export const APP_CONFIG = {
  NAME: 'DijaGold POS System',
  VERSION: '2.0.1',
  COMPANY: 'DijaGold Jewelry',
};

// Development flags
export const DEV_CONFIG = {
  MOCK_API: process.env.REACT_APP_MOCK_API === 'true',
  DEBUG_MODE: process.env.NODE_ENV === 'development',
  ENABLE_CONSOLE_LOGS: process.env.NODE_ENV === 'development',
};

// Feature flags
export const FEATURES = {
  GOLD_RATE_UPDATES: true,
  BARCODE_SCANNING: false,
  MULTI_BRANCH: true,
  LOYALTY_PROGRAM: true,
  REPAIR_TRACKING: true,
};

// Local storage keys
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'dijagold_token',
  USER_PREFERENCES: 'dijagold_preferences',
  OFFLINE_DATA: 'dijagold_offline',
};

export default {
  API_CONFIG,
  APP_CONFIG,
  DEV_CONFIG,
  FEATURES,
  STORAGE_KEYS,
};
