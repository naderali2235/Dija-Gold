export const environment = {
  production: false,
  apiUrl: '/api',
  appName: 'Dija Gold POS',
  version: '1.0.0',
  brand: {
    logoPath: 'assets/brand/dija-gold-logo.svg',
    primaryColor: '#d4af37',
    contact: {
      // Arabic address from provided image
      address: 'المقطم الهضبه الوسطى الحي الثامن قطعه ٩٠٧٧ بجانب بنزينه وطنية امام كارفور المعادي ع الدائري',
      phone1: '01515293929',
      phone2: '01558588947'
    }
  },
  
  // Application settings
  settings: {
    // Currency settings
    currency: {
      code: 'EGP',
      symbol: 'ج.م',
      decimalPlaces: 2
    },
    
    // Weight settings
    weight: {
      unit: 'g',
      decimalPlaces: 3
    },
    
    // Pagination defaults
    pagination: {
      defaultPageSize: 25,
      pageSizeOptions: [10, 25, 50, 100]
    },
    
    // Auto-refresh intervals (in seconds)
    refreshIntervals: {
      dashboard: 30,
      inventory: 60,
      goldRates: 300
    },
    
    // Local storage keys
    storage: {
      authToken: 'dija_gold_auth_token',
      userInfo: 'dija_gold_user_info',
      preferences: 'dija_gold_preferences'
    },
    
    // Receipt settings
    receipt: {
      defaultCopies: 2,
      paperWidth: 80 // mm
    }
  }
};