# Shift Settlement Feature

## Overview

The Shift Settlement feature allows users to close their shift by settling (removing) a portion of the cash drawer balance and automatically carrying forward the remaining balance to the next day's opening balance.

## Business Use Case

At the end of a shift, cashiers often need to:
1. Count the total cash in the drawer
2. Remove a portion for bank deposit or safe storage
3. Leave a smaller amount as the next day's opening balance

This feature automates this process and maintains proper audit trails.

## How It Works

### 1. Settlement Process
- User enters the total actual closing balance (all cash in drawer)
- Settlement amount is automatically set to the Expected Closing Balance (cannot be changed)
- System calculates the carry-forward amount: `Actual Balance - Expected Closing Balance`
- Current day's drawer is closed with settlement details
- Next day's drawer is automatically opened with the carry-forward amount

**Business Rule**: Settlement amount must always equal the Expected Closing Balance. This ensures only the earned revenue from the shift (sales + repairs) is settled, and any excess cash (overage) is carried forward to the next day.

### 2. Database Changes
New fields added to `CashDrawerBalance` table:
- `SettledAmount`: Amount removed during settlement
- `CarriedForwardAmount`: Amount carried to next day
- `SettlementNotes`: Reason/notes for settlement

### 3. API Endpoints

#### POST /api/cash-drawer/settle-shift
Settles the current shift and carries forward balance to next day.

**Request Body:**
```json
{
  "branchId": 1,
  "actualClosingBalance": 5000.00,
  "settledAmount": 4000.00,
  "date": "2024-01-15",
  "settlementNotes": "Bank deposit",
  "notes": "End of shift notes"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "branchId": 1,
    "balanceDate": "2024-01-15",
    "openingBalance": 1000.00,
    "expectedClosingBalance": 4800.00,
    "actualClosingBalance": 5000.00,
    "settledAmount": 4000.00,
    "carriedForwardAmount": 1000.00,
    "status": 2,
    "settlementNotes": "Bank deposit"
  }
}
```

### 4. Frontend Changes

#### Cash Drawer Component
- Added "Settle Shift" card alongside "Close Cash Drawer"
- New input fields for settlement amount and settlement notes
- Real-time calculation of carry-forward amount
- Enhanced balance display to show settlement information

#### Key Features:
- Input validation (settlement amount ≤ actual balance)
- Real-time carry-forward calculation
- Clear visual distinction between regular close and settlement
- Settlement information display in balance summary

### 5. Validation Rules

- Settlement amount must be non-negative
- Settlement amount cannot exceed actual closing balance
- Next day's drawer must not already exist
- Current day's drawer must be open

### 6. Audit Trail

All settlement operations are logged with:
- User who performed the settlement
- Timestamp of settlement
- Settlement amount and carry-forward amount
- Settlement notes/reason

## Usage Example

### Scenario:
- Opening balance: $1,000
- Cash sales during day: $4,000
- Cash repairs during day: $300
- Expected closing: $5,300 (opening + sales + repairs)
- Actual cash count: $5,500 (over by $200)
- System automatically sets settlement to $5,300 (expected closing)
- Carry forward: $200 (the overage)

### Settlement Process:
1. Enter actual closing balance: $5,200
2. Settlement amount is automatically set to: $5,000 (Expected Closing)
3. Add settlement notes: "Bank deposit - daily revenue"
4. System shows carry-forward: $200
5. Click "Settle Shift"

### Result:
- Current day closed with $200 overage
- $5,000 marked as settled (exact expected closing balance)
- Next day automatically opened with $200 opening balance (the overage)

## Benefits

1. **Automated Balance Carry-Forward**: No manual entry of next day's opening balance
2. **Audit Trail**: Complete tracking of settlements and carry-forwards
3. **Error Reduction**: Automatic calculations reduce human error
4. **Compliance**: Proper documentation for financial audits
5. **Flexibility**: Support for both regular close and settlement workflows

## Database Migration

Run the provided SQL script to add the new settlement fields:
```sql
-- See DijaGoldPOS.API/Scripts/AddSettlementFields.sql
```

## Testing Checklist

- [ ] Settlement with valid amounts
- [ ] Validation of settlement amount > actual balance
- [ ] Next day drawer creation
- [ ] Prevention of duplicate next day drawers
- [ ] Settlement information display
- [ ] Audit logging
- [ ] Error handling and user feedback
