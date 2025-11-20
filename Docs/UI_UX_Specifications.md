# EcoRide Mobile Application - UI/UX Specifications

**Version:** 1.0
**Last Updated:** November 20, 2025
**Status:** Implementation Complete - Ready for Frontend Development

---

## Table of Contents

1. [Introduction](#introduction)
2. [Design Principles](#design-principles)
3. [Color Palette & Typography](#color-palette--typography)
4. [Screen Specifications](#screen-specifications)
   - [US-001: User Registration](#us-001-user-registration)
   - [US-002: User Login](#us-002-user-login)
   - [US-003: Map & Vehicle Discovery](#us-003-map--vehicle-discovery)
   - [US-004: Vehicle Unlock](#us-004-vehicle-unlock)
   - [US-005: Active Trip View](#us-005-active-trip-view)
   - [US-006: End Trip & Payment](#us-006-end-trip--payment)
   - [US-007: Trip History](#us-007-trip-history)
   - [US-008: Wallet Management](#us-008-wallet-management)
   - [US-009: Admin Dashboard](#us-009-admin-dashboard)
5. [API Integration Guide](#api-integration-guide)
6. [Error Handling & Validation](#error-handling--validation)

---

## Introduction

This document provides comprehensive UI/UX specifications for the EcoRide mobile application based on the fully implemented backend API. All features have been developed, tested, and are ready for frontend integration.

**Backend Status:**
- All 9 User Stories implemented and tested
- 323 tests passing (250 unit, 57 integration, 16 E2E)
- Clean Architecture with CQRS pattern
- PostgreSQL database with EF Core migrations
- RESTful API with comprehensive error handling

---

## Design Principles

1. **Simplicity First**: Minimize cognitive load with clear, intuitive interfaces
2. **Eco-Friendly Aesthetics**: Use green tones to reinforce environmental mission
3. **Accessibility**: WCAG 2.1 AA compliance
4. **Mobile-First**: Optimized for iOS and Android devices
5. **Real-Time Feedback**: Immediate visual feedback for all user actions
6. **Offline-Ready**: Graceful degradation when network is unavailable

---

## Color Palette & Typography

### Primary Colors
```
Primary Green:   #00A86B (Buttons, CTA, Active states)
Dark Green:      #006B4A (Headers, Important text)
Light Green:     #E8F5E9 (Background highlights)
Success:         #4CAF50
Warning:         #FF9800
Error:           #F44336
Info:            #2196F3
```

### Neutral Colors
```
Background:      #FFFFFF
Surface:         #F5F5F5
Border:          #E0E0E0
Text Primary:    #212121
Text Secondary:  #757575
Text Disabled:   #BDBDBD
```

### Typography
```
Font Family:     Inter, SF Pro Display (iOS), Roboto (Android)

Headings:
  H1: 32px, Bold, Letter-spacing -0.5px
  H2: 24px, SemiBold, Letter-spacing -0.25px
  H3: 20px, SemiBold
  H4: 18px, Medium

Body:
  Large: 16px, Regular, Line-height 24px
  Regular: 14px, Regular, Line-height 20px
  Small: 12px, Regular, Line-height 16px

Buttons:
  Primary: 16px, SemiBold
  Secondary: 14px, Medium
```

---

## Screen Specifications

### US-001: User Registration

**Endpoint:** `POST /api/users/register`

#### Screen Layout

**Header**
- Title: "Create Account"
- Subtitle: "Join EcoRide and start your eco-friendly journey"

**Form Fields**
1. **Full Name**
   - Input type: Text
   - Placeholder: "Enter your full name"
   - Validation: Required, min 2 characters
   - Error message: "Name must be at least 2 characters"

2. **Email**
   - Input type: Email
   - Placeholder: "your.email@example.com"
   - Validation: Required, valid email format
   - Error messages:
     - "Email is required"
     - "Please enter a valid email address"
     - "This email is already registered" (from API)

3. **Phone Number**
   - Input type: Tel
   - Placeholder: "+212 6XX XXX XXX"
   - Format: Morocco phone number (+212)
   - Validation: Required, valid format
   - Error message: "Please enter a valid phone number"

4. **Password**
   - Input type: Password (with show/hide toggle)
   - Placeholder: "Create a strong password"
   - Validation: Required, min 8 characters, 1 uppercase, 1 number, 1 special char
   - Strength indicator: Weak/Medium/Strong
   - Error messages:
     - "Password is required"
     - "Password must be at least 8 characters"
     - "Include uppercase, number, and special character"

5. **Confirm Password**
   - Input type: Password
   - Placeholder: "Re-enter your password"
   - Validation: Must match password
   - Error message: "Passwords do not match"

**Call-to-Action**
- Primary Button: "Create Account"
  - Full width, 48px height
  - Background: Primary Green
  - Text: White, 16px SemiBold
  - Disabled state: Gray background when form invalid
  - Loading state: Spinner + "Creating account..."

**Secondary Actions**
- Link: "Already have an account? Sign In"
  - Text: Text Secondary, 14px
  - Action: Navigate to Login screen

**API Request**
```json
POST /api/users/register
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe",
  "phoneNumber": "+212612345678"
}
```

**Success Response**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phoneNumber": "+212612345678",
  "walletBalance": 0.00
}
```

**Success Flow**
1. Show success toast: "Account created successfully!"
2. Auto-navigate to Map screen
3. Store userId in secure storage

**Error Handling**
- Email already exists (400): Show inline error under email field
- Validation errors (400): Show inline errors under respective fields
- Server error (500): Show error modal with retry option

---

### US-002: User Login

**Endpoint:** `POST /api/users/login`

#### Screen Layout

**Header**
- Logo: EcoRide logo (centered, 80px height)
- Title: "Welcome Back"
- Subtitle: "Login to continue your journey"

**Form Fields**
1. **Email**
   - Input type: Email
   - Placeholder: "your.email@example.com"
   - Validation: Required, valid email
   - Auto-fill support: Yes

2. **Password**
   - Input type: Password (with show/hide toggle)
   - Placeholder: "Enter your password"
   - Validation: Required

**Call-to-Action**
- Primary Button: "Sign In"
  - Full width, 48px height
  - Background: Primary Green
  - Loading state: Spinner + "Signing in..."

**Secondary Actions**
- Link: "Forgot Password?" (aligned right)
- Link: "Don't have an account? Sign Up" (centered, bottom)

**API Request**
```json
POST /api/users/login
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Success Response**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phoneNumber": "+212612345678",
  "walletBalance": 150.00
}
```

**Success Flow**
1. Store userId in secure storage
2. Navigate to Map screen
3. Show welcome toast: "Welcome back, {fullName}!"

**Error Handling**
- Invalid credentials (400): Show error: "Invalid email or password"
- Account not found (404): Show error: "No account found with this email"
- Server error (500): Show error modal with retry

---

### US-003: Map & Vehicle Discovery

**Endpoints:**
- `GET /api/vehicles/available?latitude={lat}&longitude={lng}&radiusKm={radius}`
- `GET /api/vehicles/nearby?latitude={lat}&longitude={lng}&radiusKm={radius}`

#### Screen Layout

**Map View**
- Full-screen interactive map (Google Maps/Mapbox)
- User location: Blue pulsing circle
- Vehicle markers:
  - Available Bike: Green bike icon with battery %
  - Available Scooter: Green scooter icon with battery %
  - Unavailable: Gray icon with "In Use" badge
- Cluster markers when zoomed out (show count)
- Current location button (bottom right)

**Top Bar** (overlaid on map)
- User avatar (top left) → Profile menu
- Wallet balance badge (top right)
  - Display: "{balance} MAD"
  - Tap to open Wallet screen
  - Color indicator:
    - Green: balance > 50 MAD
    - Orange: balance 10-50 MAD
    - Red: balance < 10 MAD

**Search Bar** (below top bar)
- Placeholder: "Search location or address"
- Auto-complete suggestions
- Current location chip

**Filter Bar** (below search)
- Chips:
  - "All" (default selected)
  - "Bikes"
  - "Scooters"
  - "Battery > 50%"
- Horizontal scroll for more filters

**Bottom Sheet** (draggable)

**State 1: Collapsed** (peek height: 120px)
- Title: "Available Vehicles Nearby"
- Count: "{count} vehicles available"
- Drag handle (centered top)

**State 2: Half-Expanded** (50% screen)
- List of nearby vehicles (sorted by distance)

**Vehicle Card:**
```
┌─────────────────────────────────────┐
│ [Icon] Scooter #ECO-S001            │
│        Battery: 85%                 │
│        150m away • ~2 min walk      │
│        Last service: 2 days ago     │
│        [Unlock] button              │
└─────────────────────────────────────┘
```

**State 3: Fully Expanded** (80% screen)
- Same list with map preview for each vehicle
- Pull to refresh

**Vehicle Card Interactions**
- Tap card: Center map on vehicle + show details
- Tap "Unlock": Navigate to Unlock screen with vehicle ID

**API Request**
```
GET /api/vehicles/nearby?latitude=33.5731&longitude=-7.5898&radiusKm=2
```

**Success Response**
```json
{
  "vehicles": [
    {
      "vehicleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "ECO-S001",
      "type": "Scooter",
      "batteryLevel": 85,
      "location": {
        "latitude": 33.5731,
        "longitude": -7.5898
      },
      "isAvailable": true,
      "distanceInMeters": 150,
      "estimatedWalkingTimeMinutes": 2
    }
  ],
  "totalCount": 15
}
```

**Empty State**
- Icon: Sad vehicle illustration
- Message: "No vehicles available nearby"
- Suggestion: "Try expanding your search radius or check back later"
- Action: "Expand Search" button

**Error Handling**
- Location permission denied: Show permission request dialog
- Network error: Show offline banner with retry
- API error: Show error toast with retry option

---

### US-004: Vehicle Unlock

**Endpoint:** `POST /api/vehicles/unlock`

#### Screen Layout

**Header**
- Back button (top left)
- Title: "Unlock Vehicle"
- Vehicle code: "ECO-S001" (subtitle)

**Vehicle Preview**
- Large vehicle image/illustration
- Battery indicator (circular progress)
  - Value: "85%"
  - Color: Green (>50%), Orange (25-50%), Red (<25%)
- Status badge: "Available"

**Vehicle Details Card**
```
┌─────────────────────────────────────┐
│ Vehicle Information                 │
│ ─────────────────────────────────── │
│ Type:         Scooter               │
│ Battery:      85%                   │
│ Range:        ~12 km                │
│ Location:     150m away             │
│ Last cleaned: Today                 │
└─────────────────────────────────────┘
```

**Pricing Information Card**
```
┌─────────────────────────────────────┐
│ Pricing                             │
│ ─────────────────────────────────── │
│ Base cost:    5.00 MAD              │
│ Per minute:   1.50 MAD              │
│                                     │
│ Example: 10 min ride = 20.00 MAD    │
└─────────────────────────────────────┘
```

**Wallet Balance Check**
```
┌─────────────────────────────────────┐
│ Your Wallet: 150.00 MAD             │
│ ✓ Sufficient balance                │
└─────────────────────────────────────┘
```

**If balance < 10 MAD:**
```
┌─────────────────────────────────────┐
│ Your Wallet: 5.00 MAD               │
│ ⚠ Insufficient balance              │
│ [Top Up Wallet] button              │
└─────────────────────────────────────┘
```

**Scan QR Code Section**
- Button: "Scan QR Code"
  - Icon: QR code icon
  - Opens camera with QR scanner overlay
  - Auto-submit when QR detected

**Or**

**Enter Code Manually**
- Input: Vehicle code
- Placeholder: "Enter vehicle code (e.g., ECO-S001)"
- Validation: Required, format ECO-[A-Z]\d{3}

**Call-to-Action**
- Primary Button: "Unlock Vehicle"
  - Full width, 56px height
  - Background: Primary Green
  - Disabled if: insufficient balance OR invalid code
  - Loading state: Spinner + "Unlocking..."

**API Request**
```json
POST /api/vehicles/unlock
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleCode": "ECO-S001"
}
```

**Success Response**
```json
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleCode": "ECO-S001",
  "startedAt": "2025-11-20T10:30:00Z",
  "message": "Vehicle unlocked successfully. Enjoy your ride!"
}
```

**Success Flow**
1. Show success modal:
   - Icon: Checkmark animation
   - Message: "Vehicle Unlocked!"
   - Subtext: "Enjoy your ride. Stay safe!"
   - Auto-close after 2 seconds
2. Navigate to Active Trip screen
3. Start trip timer

**Error Handling**
- Vehicle not found (404): "Vehicle not found. Please check the code."
- Vehicle unavailable (400): "This vehicle is currently in use. Please choose another."
- Insufficient balance (400): Show top-up prompt
- User has active trip (400): "You already have an active trip. End it first."
- Server error (500): Show retry option

---

### US-005: Active Trip View

**Endpoint:** `GET /api/trips/active?userId={userId}`

#### Screen Layout

**This is a full-screen overlay that appears during an active trip.**

**Map View**
- Show current location with navigation route
- Vehicle destination marker (if set)
- Real-time location updates every 5 seconds

**Top Info Card** (overlaid, semi-transparent)
```
┌─────────────────────────────────────┐
│ Trip in Progress                    │
│ ECO-S001 • Scooter                  │
│                                     │
│ Duration: 00:12:34 (live timer)     │
│ Distance: 2.5 km                    │
│ Est. Cost: 23.50 MAD (updating)     │
└─────────────────────────────────────┘
```

**Cost Calculation Display**
```
Base cost:    5.00 MAD
Time cost:    18.50 MAD (12.5 min × 1.50)
Total:        23.50 MAD
```

**Bottom Action Sheet**

**Primary Actions**
- Button: "End Trip"
  - Background: Error Red
  - Icon: Stop icon
  - Action: Show confirmation dialog

**Secondary Actions**
- Button: "Report Issue"
  - Background: Warning Orange
  - Options:
    - Vehicle damaged
    - Low battery
    - Other issue

**Confirmation Dialog** (when "End Trip" tapped)
```
┌─────────────────────────────────────┐
│ End Trip?                           │
│                                     │
│ Duration: 12:34                     │
│ Total Cost: 23.50 MAD               │
│                                     │
│ Are you sure you want to end this   │
│ trip? You will be charged.          │
│                                     │
│ [Cancel]  [End Trip]                │
└─────────────────────────────────────┘
```

**API Request**
```
GET /api/trips/active?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Success Response**
```json
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleCode": "ECO-S001",
  "vehicleType": "Scooter",
  "startedAt": "2025-11-20T10:30:00Z",
  "durationMinutes": 12,
  "estimatedCost": 23.50
}
```

**Real-Time Updates**
- Timer updates every second
- Cost recalculates every minute
- Location updates every 5 seconds
- Battery level updates every minute

**Error Handling**
- No active trip (404): Redirect to Map screen
- Network loss: Show offline banner, cache current state
- GPS unavailable: Show warning, continue with last known location

---

### US-006: End Trip & Payment

**Endpoint:** `POST /api/trips/end`

#### Screen Flow

**Step 1: Trip Summary**

**Header**
- Title: "Trip Completed!"
- Confetti animation (brief)

**Trip Details Card**
```
┌─────────────────────────────────────┐
│ Trip Summary                        │
│ ─────────────────────────────────── │
│ Vehicle:      ECO-S001 (Scooter)    │
│ Started:      10:30 AM              │
│ Ended:        10:42 AM              │
│ Duration:     12 minutes            │
│ Distance:     2.5 km                │
│                                     │
│ Base cost:    5.00 MAD              │
│ Time cost:    18.00 MAD             │
│ ─────────────────────────────────── │
│ Total:        23.00 MAD             │
└─────────────────────────────────────┘
```

**Payment Method**
```
┌─────────────────────────────────────┐
│ Payment                             │
│ ─────────────────────────────────── │
│ [●] Wallet Balance                  │
│     Current: 150.00 MAD             │
│     After:   127.00 MAD             │
│                                     │
│ [ ] Credit Card (coming soon)       │
└─────────────────────────────────────┘
```

**Step 2: Rate Your Trip**

**Star Rating**
- 5-star rating component (large, tappable)
- Label: "How was your ride?"
- Default: No stars selected

**Optional Feedback**
- Text area: "Share your experience (optional)"
- Max length: 500 characters
- Placeholder: "Tell us about your trip..."

**Call-to-Action**
- Primary Button: "Submit & Pay"
  - Disabled until rating selected
  - Loading state: "Processing payment..."

**API Request**
```json
POST /api/trips/end
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "endLocation": {
    "latitude": 33.5750,
    "longitude": -7.5900
  },
  "rating": 5,
  "feedback": "Great ride, smooth scooter!"
}
```

**Success Response**
```json
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalCost": 23.00,
  "durationMinutes": 12,
  "rating": 5,
  "newWalletBalance": 127.00,
  "message": "Payment successful. Thank you for riding with EcoRide!"
}
```

**Success Flow**
1. Show success animation (checkmark)
2. Display: "Payment Successful!"
3. Update wallet balance in UI
4. Option to:
   - "View Receipt" → Navigate to Receipt screen
   - "Start New Ride" → Navigate to Map screen
5. Auto-redirect to Map after 5 seconds

**Error Handling**
- Insufficient balance (400):
  - Show error: "Insufficient wallet balance"
  - Action: "Top Up Wallet" button
- Payment processing error (500):
  - Show error: "Payment failed. Please try again."
  - Action: Retry button
- Trip not found (404): Show error and redirect to Map

---

### US-007: Trip History

**Endpoints:**
- `GET /api/trips/history?userId={userId}&pageNumber={page}&pageSize={size}`
- `GET /api/trips/{tripId}?userId={userId}`
- `GET /api/trips/{tripId}/receipt?userId={userId}`

#### Screen Layout

**Header**
- Title: "Trip History"
- Back button (top left)
- Filter icon (top right)

**Filter Sheet** (opens from top right icon)
```
┌─────────────────────────────────────┐
│ Filters                             │
│ ─────────────────────────────────── │
│ Date Range:                         │
│ [ ] Last 7 days                     │
│ [ ] Last 30 days                    │
│ [ ] Last 3 months                   │
│ [●] All time                        │
│                                     │
│ Vehicle Type:                       │
│ [●] All                             │
│ [ ] Bikes only                      │
│ [ ] Scooters only                   │
│                                     │
│ [Clear All]  [Apply]                │
└─────────────────────────────────────┘
```

**Statistics Card** (top of list)
```
┌─────────────────────────────────────┐
│ Your Riding Stats                   │
│ ─────────────────────────────────── │
│ 45 trips     120 km     12.5 hours  │
│ Total Spent: 680.00 MAD             │
└─────────────────────────────────────┘
```

**Trip List**

**List Item:**
```
┌─────────────────────────────────────┐
│ Nov 20, 2025 • 10:30 AM             │
│                                     │
│ [Scooter Icon] ECO-S001             │
│ 12 min • 2.5 km • 5★                │
│                                     │
│ 23.00 MAD                           │
│ [Receipt >]                         │
└─────────────────────────────────────┘
```

**Tap Behavior:**
- Tap card: Navigate to Trip Details
- Tap "Receipt": Navigate to Receipt screen

**Pagination**
- Load 20 trips per page
- Infinite scroll (load more at bottom)
- Pull to refresh

**API Request**
```
GET /api/trips/history?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6&pageNumber=1&pageSize=20
```

**Success Response**
```json
{
  "trips": [
    {
      "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
      "vehicleCode": "ECO-S001",
      "vehicleType": "Scooter",
      "startedAt": "2025-11-20T10:30:00Z",
      "endedAt": "2025-11-20T10:42:00Z",
      "durationMinutes": 12,
      "totalCost": 23.00,
      "rating": 5
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

#### Trip Details Screen

**Endpoint:** `GET /api/trips/{tripId}?userId={userId}`

**Header**
- Back button
- Title: "Trip Details"
- Share icon (top right) → Share receipt

**Map Preview**
- Show route from start to end location
- Start marker (green pin)
- End marker (red pin)
- Route polyline

**Details Card**
```
┌─────────────────────────────────────┐
│ Trip Information                    │
│ ─────────────────────────────────── │
│ Vehicle:      ECO-S001 (Scooter)    │
│ Date:         Nov 20, 2025          │
│ Started:      10:30 AM              │
│ Ended:        10:42 AM              │
│ Duration:     12 minutes            │
│ Distance:     2.5 km                │
│ Avg Speed:    12.5 km/h             │
│                                     │
│ Start:        123 Main St           │
│ End:          456 Park Ave          │
│                                     │
│ Rating:       ★★★★★ (5/5)           │
│ Feedback:     "Great ride!"         │
└─────────────────────────────────────┘
```

**Cost Breakdown**
```
┌─────────────────────────────────────┐
│ Cost Breakdown                      │
│ ─────────────────────────────────── │
│ Base cost:           5.00 MAD       │
│ Time cost:          18.00 MAD       │
│ (12 min × 1.50)                     │
│ ─────────────────────────────────── │
│ Total:              23.00 MAD       │
│                                     │
│ Payment: Wallet                     │
│ Status: Paid ✓                      │
└─────────────────────────────────────┘
```

**Actions**
- Button: "View Receipt"
- Button: "Report Issue" (if trip within 24h)

**API Request**
```
GET /api/trips/8fa85f64-5717-4562-b3fc-2c963f66afa6?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Success Response**
```json
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "vehicleCode": "ECO-S001",
  "vehicleType": "Scooter",
  "startedAt": "2025-11-20T10:30:00Z",
  "endedAt": "2025-11-20T10:42:00Z",
  "durationMinutes": 12,
  "startLocation": {
    "latitude": 33.5731,
    "longitude": -7.5898,
    "address": "123 Main St"
  },
  "endLocation": {
    "latitude": 33.5750,
    "longitude": -7.5900,
    "address": "456 Park Ave"
  },
  "baseCost": 5.00,
  "timeCost": 18.00,
  "totalCost": 23.00,
  "rating": 5,
  "feedback": "Great ride!",
  "paymentMethod": "Wallet",
  "receiptAvailable": true
}
```

#### Receipt Screen

**Endpoint:** `GET /api/trips/{tripId}/receipt?userId={userId}`

**Header**
- Back button
- Title: "Receipt"
- Download icon (save as PDF)
- Share icon (share receipt)

**Receipt Design** (printable format)
```
┌─────────────────────────────────────┐
│        ECORIDE                      │
│    Eco-Friendly Mobility            │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ TRIP RECEIPT                        │
│                                     │
│ Receipt #: TR-2025-11-20-0042       │
│ Date: November 20, 2025             │
│ Time: 10:42 AM                      │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ TRIP DETAILS                        │
│                                     │
│ Vehicle:      ECO-S001 (Scooter)    │
│ Trip ID:      8fa85f64...           │
│ Duration:     12 minutes            │
│ Distance:     2.5 km                │
│                                     │
│ Started:      10:30 AM              │
│               123 Main St           │
│                                     │
│ Ended:        10:42 AM              │
│               456 Park Ave          │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ CHARGES                             │
│                                     │
│ Base cost                  5.00 MAD │
│ Time (12 min × 1.50)      18.00 MAD │
│                                     │
│ ─────────────────────────────────── │
│ TOTAL                     23.00 MAD │
│ ─────────────────────────────────── │
│                                     │
│ Payment Method: Wallet              │
│ Status: Paid ✓                      │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ Thank you for choosing EcoRide!     │
│ Help us protect the environment.    │
│                                     │
│ Questions? support@ecoride.ma       │
│                                     │
└─────────────────────────────────────┘
```

**Actions**
- Button: "Download PDF"
- Button: "Share via Email"
- Button: "Share via WhatsApp"

**API Request**
```
GET /api/trips/8fa85f64-5717-4562-b3fc-2c963f66afa6/receipt?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Success Response**
```json
{
  "tripId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiptNumber": "TR-2025-11-20-0042",
  "issueDate": "2025-11-20T10:42:00Z",
  "vehicleCode": "ECO-S001",
  "vehicleType": "Scooter",
  "startTime": "2025-11-20T10:30:00Z",
  "endTime": "2025-11-20T10:42:00Z",
  "durationMinutes": 12,
  "baseCost": 5.00,
  "timeCost": 18.00,
  "totalCost": 23.00,
  "paymentMethod": "Wallet",
  "startAddress": "123 Main St",
  "endAddress": "456 Park Ave"
}
```

**Empty State** (no trips)
- Icon: Empty folder illustration
- Message: "No trips yet"
- Subtext: "Your trip history will appear here"
- Action: "Start Your First Ride" button → Navigate to Map

---

### US-008: Wallet Management

**Endpoints:**
- `GET /api/wallet/balance?userId={userId}`
- `POST /api/wallet/add-funds`
- `GET /api/wallet/transactions?userId={userId}&pageNumber={page}&pageSize={size}`

#### Wallet Screen Layout

**Header**
- Back button (top left)
- Title: "My Wallet"
- Settings icon (top right) → Payment methods

**Balance Card** (prominent, top of screen)
```
┌─────────────────────────────────────┐
│ Current Balance                     │
│                                     │
│ 150.00 MAD                          │
│                                     │
│ Last updated: Just now              │
│ [Refresh icon]                      │
└─────────────────────────────────────┘
```

**Quick Top-Up Section**
```
┌─────────────────────────────────────┐
│ Quick Add Funds                     │
│ ─────────────────────────────────── │
│ [50 MAD]  [100 MAD]  [200 MAD]      │
│                                     │
│ Or enter custom amount:             │
│ [___________] MAD                   │
│                                     │
│ Min: 10 MAD • Max: 1000 MAD         │
│                                     │
│ Payment Method:                     │
│ [●] Credit Card ****1234            │
│ [ ] Debit Card ****5678             │
│ [+ Add New Card]                    │
│                                     │
│ [Add Funds] button                  │
└─────────────────────────────────────┘
```

**Quick Amount Chips**
- Tappable chips: 50, 100, 200 MAD
- When tapped:
  - Highlight selected chip
  - Auto-fill custom amount field
  - Enable "Add Funds" button

**Custom Amount Input**
- Input type: Number
- Placeholder: "Enter amount"
- Validation:
  - Minimum: 10 MAD
  - Maximum: 1000 MAD
  - Decimal: Up to 2 places
- Real-time validation messages:
  - Below 10: "Minimum amount is 10 MAD"
  - Above 1000: "Maximum amount is 1000 MAD"
  - Invalid format: "Please enter a valid amount"

**Add Funds Button**
- Full width, 48px height
- Background: Primary Green
- Disabled states:
  - No amount entered
  - Amount invalid (< 10 or > 1000)
  - No payment method selected
- Loading state: "Processing..."

**Transaction History Section**
```
┌─────────────────────────────────────┐
│ Recent Transactions                 │
│ ─────────────────────────────────── │
│                                     │
│ Nov 20, 10:42 AM                    │
│ Trip Payment • ECO-S001             │
│ - 23.00 MAD                         │
│ Balance: 150.00 MAD                 │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ Nov 20, 9:15 AM                     │
│ Wallet Top-Up • Card ****1234       │
│ + 100.00 MAD                        │
│ Balance: 173.00 MAD                 │
│                                     │
│ ─────────────────────────────────── │
│                                     │
│ Nov 19, 4:30 PM                     │
│ Trip Payment • ECO-B012             │
│ - 15.00 MAD                         │
│ Balance: 73.00 MAD                  │
│                                     │
│ [View All Transactions]             │
└─────────────────────────────────────┘
```

**Transaction Item Design**
```
┌─────────────────────────────────────┐
│ [Icon] Transaction Type             │
│        Details                      │
│        Amount (+ or -)              │
│        Balance after                │
│        Date/Time                    │
└─────────────────────────────────────┘
```

**Transaction Types:**
1. **Top-Up**
   - Icon: + in green circle
   - Color: Green text
   - Amount: +100.00 MAD

2. **Trip Payment**
   - Icon: - in red circle
   - Color: Red text
   - Amount: -23.00 MAD

**API Requests**

**Get Balance:**
```
GET /api/wallet/balance?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "balance": 150.00,
  "lastUpdated": "2025-11-20T10:42:00Z"
}
```

**Add Funds:**
```json
POST /api/wallet/add-funds
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100.00,
  "paymentMethodId": "pm_1234567890"
}
```

**Success Response:**
```json
{
  "transactionId": "tx_987654321",
  "amount": 100.00,
  "newBalance": 250.00,
  "timestamp": "2025-11-20T11:00:00Z",
  "message": "Funds added successfully"
}
```

**Success Flow:**
1. Show success animation
2. Display: "100.00 MAD added to your wallet"
3. Update balance card immediately
4. Add new transaction to history list
5. Clear input fields
6. Auto-close after 2 seconds

**Error Handling:**
- Amount < 10 MAD (400):
  - Show error: "Minimum amount is 10 MAD"
  - Highlight input field in red
- Amount > 1000 MAD (400):
  - Show error: "Maximum amount is 1000 MAD"
  - Highlight input field in red
- Payment failed (400):
  - Show modal: "Payment Failed"
  - Message: "We couldn't process your payment. Please try again or use a different payment method."
  - Actions: [Try Again] [Change Payment Method]
- Server error (500):
  - Show error toast with retry option

#### Transaction History Screen

**Endpoint:** `GET /api/wallet/transactions?userId={userId}&pageNumber={page}&pageSize={size}`

**Header**
- Back button
- Title: "Transaction History"
- Filter icon (top right)

**Filter Options**
- All Transactions
- Top-Ups Only
- Payments Only
- Date Range selector

**Summary Card** (top)
```
┌─────────────────────────────────────┐
│ Summary                             │
│ ─────────────────────────────────── │
│ Total Topped Up:    500.00 MAD      │
│ Total Spent:        350.00 MAD      │
│ Current Balance:    150.00 MAD      │
└─────────────────────────────────────┘
```

**Transaction List**
- Full list with pagination
- 20 items per page
- Pull to refresh
- Infinite scroll

**API Request**
```
GET /api/wallet/transactions?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6&pageNumber=1&pageSize=20
```

**Success Response**
```json
{
  "transactions": [
    {
      "transactionId": "tx_123",
      "amount": -23.00,
      "transactionType": "TripPayment",
      "paymentMethod": "Wallet",
      "paymentDetails": "ECO-S001",
      "balanceBefore": 173.00,
      "balanceAfter": 150.00,
      "createdAt": "2025-11-20T10:42:00Z"
    },
    {
      "transactionId": "tx_124",
      "amount": 100.00,
      "transactionType": "TopUp",
      "paymentMethod": "CreditCard",
      "paymentDetails": "****1234",
      "balanceBefore": 73.00,
      "balanceAfter": 173.00,
      "createdAt": "2025-11-20T09:15:00Z"
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

**Empty State**
- Icon: Empty wallet illustration
- Message: "No transactions yet"
- Subtext: "Your wallet activity will appear here"

---

### US-009: Admin Dashboard

**Endpoint:** `GET /api/admin/dashboard`

**Note:** This is a web-based admin panel, not part of the mobile app.

#### Dashboard Layout

**Sidebar Navigation**
- Logo: EcoRide Admin
- Menu items:
  - Dashboard (home)
  - Users
  - Vehicles
  - Trips
  - Financials
  - Settings

**Top Bar**
- Search: "Search users, vehicles, trips..."
- Notifications icon (with badge)
- Admin profile menu

**Main Content Area**

**Statistics Cards** (top row)
```
┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│ Total Users   │ │ Total Vehicles│ │ Active Trips  │ │ Total Revenue │
│               │ │               │ │               │ │               │
│   1,234       │ │     150       │ │      12       │ │  45,680 MAD   │
│   +12% ↑      │ │     +5% ↑     │ │     -3% ↓     │ │   +18% ↑      │
└───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘
```

**Charts Section**
```
┌─────────────────────────────────────┐ ┌───────────────────────────┐
│ Revenue Trend (Last 30 Days)        │ │ Trip Distribution         │
│                                     │ │                           │
│ [Line Chart]                        │ │ [Pie Chart]               │
│ - Daily revenue                     │ │ - Bikes: 60%              │
│ - Trip count                        │ │ - Scooters: 40%           │
│                                     │ │                           │
└─────────────────────────────────────┘ └───────────────────────────┘
```

**Recent Activity** (bottom)
```
┌─────────────────────────────────────┐
│ Recent Trips                        │
│ ─────────────────────────────────── │
│ ID       User      Vehicle    Cost  │
│ TR-0042  John Doe  ECO-S001  23 MAD │
│ TR-0041  Jane S.   ECO-B012  15 MAD │
│ TR-0040  Mike J.   ECO-S002  28 MAD │
│                                     │
│ [View All Trips]                    │
└─────────────────────────────────────┘
```

**API Request**
```
GET /api/admin/dashboard
```

**Success Response**
```json
{
  "totalUsers": 1234,
  "totalVehicles": 150,
  "activeTrips": 12,
  "completedTrips": 4568,
  "totalRevenue": 45680.00,
  "lastUpdated": "2025-11-20T12:00:00Z"
}
```

**Refresh Interval**
- Auto-refresh every 30 seconds
- Manual refresh button
- Last updated timestamp

---

## API Integration Guide

### Base URL
```
Production: https://api.ecoride.ma
Staging: https://staging-api.ecoride.ma
Development: http://localhost:5000
```

### Authentication
All endpoints (except register/login) require userId in request.

**Future Enhancement:** Implement JWT authentication
```
Authorization: Bearer {jwt_token}
```

### Request Headers
```
Content-Type: application/json
Accept: application/json
```

### Response Format

**Success Response:**
```json
{
  "data": { ... },
  "message": "Success message (optional)"
}
```

**Error Response:**
```json
{
  "error": "Human-readable error message",
  "code": "ERROR_CODE",
  "details": { ... } // Optional
}
```

### HTTP Status Codes
- 200: Success
- 201: Created
- 400: Bad Request (validation error)
- 401: Unauthorized
- 403: Forbidden
- 404: Not Found
- 500: Internal Server Error

### Error Codes Reference

**User Errors:**
- `USER_NOT_FOUND`: User does not exist
- `EMAIL_ALREADY_EXISTS`: Email is already registered
- `INVALID_CREDENTIALS`: Login failed
- `WEAK_PASSWORD`: Password doesn't meet requirements

**Vehicle Errors:**
- `VEHICLE_NOT_FOUND`: Vehicle doesn't exist
- `VEHICLE_UNAVAILABLE`: Vehicle is currently in use
- `INVALID_VEHICLE_CODE`: Invalid QR code format

**Trip Errors:**
- `ACTIVE_TRIP_EXISTS`: User already has an active trip
- `NO_ACTIVE_TRIP`: No active trip found
- `TRIP_NOT_FOUND`: Trip doesn't exist
- `UNAUTHORIZED_ACCESS`: User doesn't own this trip

**Wallet Errors:**
- `INSUFFICIENT_BALANCE`: Wallet balance too low
- `MIN_AMOUNT`: Below minimum top-up amount
- `MAX_AMOUNT`: Above maximum top-up amount
- `PAYMENT_FAILED`: Payment processing failed

**Pagination Errors:**
- `INVALID_PAGE_NUMBER`: Page number < 1
- `INVALID_PAGE_SIZE`: Page size < 1 or > 100

### Rate Limiting
- 100 requests per minute per user
- 429 status code when exceeded
- Retry-After header included

---

## Error Handling & Validation

### Client-Side Validation

**Before API Call:**
1. Validate required fields
2. Check format constraints
3. Verify min/max values
4. Show inline errors immediately

**Benefits:**
- Better UX (instant feedback)
- Reduced API calls
- Lower server load

### Server-Side Validation

**Always validate on server:**
- Never trust client data
- Enforce business rules
- Return specific error codes

### Error Display Patterns

**Inline Errors** (preferred for forms)
- Show below the invalid field
- Red text color
- Icon: ⚠️
- Clear on field change

**Toast Messages** (for general errors)
- Duration: 3-5 seconds
- Position: Top of screen
- Colors:
  - Error: Red background
  - Success: Green background
  - Warning: Orange background
  - Info: Blue background

**Modal Dialogs** (for critical errors)
- Require user acknowledgment
- Block UI until dismissed
- Use sparingly

**Error Recovery**

**Network Errors:**
- Show offline banner
- Queue requests when possible
- Retry automatically (with exponential backoff)
- Allow manual retry

**Validation Errors:**
- Highlight fields in red
- Show specific error messages
- Allow correction without refresh

**Server Errors:**
- Log to error tracking service
- Show user-friendly message
- Provide support contact option

### Accessibility

**WCAG 2.1 AA Compliance:**
- Color contrast ratio ≥ 4.5:1
- Touch targets ≥ 44×44 pt
- Screen reader support
- Focus indicators
- Alt text for images
- Semantic HTML

**Testing:**
- iOS VoiceOver
- Android TalkBack
- Color blindness simulation

---

## Implementation Summary

### Completed Features

**Authentication & User Management:**
- ✅ User Registration (US-001)
- ✅ User Login (US-002)
- ✅ Wallet integration with user profile

**Vehicle Management:**
- ✅ Vehicle discovery by location (US-003)
- ✅ Real-time availability status
- ✅ QR code unlock (US-004)
- ✅ Battery level monitoring

**Trip Management:**
- ✅ Active trip tracking (US-005)
- ✅ End trip with payment (US-006)
- ✅ Trip rating system
- ✅ Trip history with pagination (US-007)
- ✅ Trip details view
- ✅ Receipt generation

**Wallet System:**
- ✅ Wallet balance display (US-008)
- ✅ Add funds (10-1000 MAD validation)
- ✅ Transaction history with pagination
- ✅ Automatic payment processing

**Admin Panel:**
- ✅ Platform monitoring dashboard (US-009)
- ✅ Real-time statistics
- ✅ Trip analytics

### Test Coverage
- ✅ 250 unit tests
- ✅ 57 integration tests
- ✅ 16 E2E tests
- ✅ 100% business logic coverage

### Architecture Highlights
- Clean Architecture (DDD)
- CQRS with MediatR
- Repository Pattern
- PostgreSQL database
- Entity Framework Core
- Result<T> pattern for error handling

---

## Next Steps for Frontend Team

### Phase 1: Foundation (Week 1-2)
1. Set up React Native / Flutter project
2. Implement design system (colors, typography, components)
3. Configure API client with error handling
4. Set up state management (Redux/MobX/Bloc)

### Phase 2: Core Features (Week 3-5)
1. Authentication screens (Register, Login)
2. Map integration (Google Maps/Mapbox)
3. Vehicle discovery and unlock
4. Active trip tracking

### Phase 3: Secondary Features (Week 6-7)
1. Trip history and details
2. Wallet management
3. Receipt generation
4. User profile

### Phase 4: Polish (Week 8)
1. Error handling refinement
2. Loading states and animations
3. Accessibility improvements
4. Performance optimization

### Phase 5: Admin Panel (Week 9-10)
1. Web-based admin dashboard
2. Analytics and reporting
3. User/vehicle management

### Recommended Stack

**Mobile:**
- React Native or Flutter
- Maps: Google Maps SDK
- QR: react-native-qrcode-scanner
- HTTP: Axios
- State: Redux Toolkit / Bloc

**Admin Web:**
- React.js or Vue.js
- Charts: Recharts / Chart.js
- UI: Material-UI / Ant Design

### Testing Strategy
- Unit tests: Jest / Flutter Test
- E2E tests: Detox / Appium
- Visual regression: Percy / Chromatic

---

## Contact & Support

**Backend API Repository:** https://github.com/Omar1Ach/EcoRide-Backend

**Questions?** Create an issue in the repository.

**API Documentation:** Available via Swagger at `/swagger`

---

**Document End**

This specification is based on the fully implemented and tested EcoRide backend API. All endpoints are functional and ready for frontend integration.
