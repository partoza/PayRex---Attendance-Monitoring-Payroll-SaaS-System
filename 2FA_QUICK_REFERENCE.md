# PayRex 2FA/TOTP Quick Reference Guide

## Overview
Two-Factor Authentication (2FA) in PayRex uses TOTP (Time-based One-Time Password) with authenticator apps.

## Supported Authenticator Apps
- ? Google Authenticator
- ? Microsoft Authenticator
- ? Authy
- ? FreeOTP
- ? Any RFC 6238 compatible TOTP app

## Setup Flow

### Step 1: Start 2FA Setup
```
Navigate to: Profile ? Two-Factor Authentication
Click: "Enable Two-Factor Authentication"
```

### Step 2: See QR Code and Secret Key
You'll see:
- QR Code (scannable)
- Manual Entry Key: `JBSW Y3DP EHPK 3PXP`

**Important:** Save the manual entry key somewhere safe before proceeding.

### Step 3: Scan QR Code
Open your authenticator app (Google Authenticator, etc.) and:
- Tap the "+" button to add new account
- Choose "Scan QR code" 
- Scan the displayed QR code
- The app will show a 6-digit number that changes every 30 seconds

**OR manually enter the key:**
- Tap the "+" button
- Choose "Enter a setup key"
- Enter: `JBSWY3DPEHPK3PXP` (no spaces)
- Name: PayRex
- Key type: Time-based

### Step 4: Verify Code
```
Look at your authenticator app
See the 6-digit code (changes every 30 seconds)
Enter the code in the "Enter Verification Code" field
Click: "Verify & Enable"
```

### Step 5: Save Recovery Codes
? **2FA is now enabled!**

You'll see 10 recovery codes displayed. These are one-time use codes that let you log in if you lose access to your authenticator app.

**Actions:**
- Copy all codes and store them securely (password manager, safe, etc.)
- Do NOT share these codes
- Each code can only be used once

---

## Testing Your Setup

1. Log out of PayRex
2. Log back in with your username/password
3. On the 2FA screen, enter the current 6-digit code from your authenticator app
4. You should be logged in successfully

---

## If You Lose Your Authenticator App

### Option 1: Use Recovery Codes
1. On login page, choose "Use recovery code" option
2. Enter one of your 10 recovery codes
3. This code is now consumed (can't be reused)
4. After logging in, go to Profile ? 2FA and re-setup with a new app

### Option 2: Contact Administrator
- If you've used all recovery codes, contact an administrator
- They can disable 2FA from the backend

---

## Disabling 2FA

Go to: **Profile ? Two-Factor Authentication**

Click: **"Disable Two-Factor Authentication"**

Confirm when prompted.

**What happens:**
- ? Secret key is deleted from database
- ? All recovery codes are deleted
- ? You can now log in with password only

---

## Troubleshooting

### Code Says "Invalid"
**Possible causes:**
- Code expired (codes last only 30 seconds)
- Clock mismatch between device and server

**Solution:** Wait for the next code to appear and try again

### Can't Find Setup Button
**Check:**
1. Are you logged in?
2. Go to top-right menu ? Profile
3. Look for "Two-Factor Authentication" section
4. If 2FA is enabled, you'll see the "Disable" button instead

### Lost Recovery Codes
If you didn't save them before closing the dialog:
1. Disable 2FA (if you can log in)
2. Re-enable it and save the new codes this time
3. If you can't log in, contact administrator

### Time-Based Code Sync Issues
If codes keep being invalid:
1. Check that your phone time is set to automatic
2. Ensure server time is correct
3. Try disabling/re-enabling 2FA

---

## Security Tips

? **Do:**
- Save recovery codes in a secure location
- Use a strong master password for your authenticator app
- Use an authenticator app (more secure than SMS)

? **Don't:**
- Share your recovery codes
- Take screenshots of the QR code
- Share your secret key
- Write codes down insecurely

---

## Pre-configured Test Account

**Email:** partozajohnrex@gmail.com  
**2FA Status:** Enabled  
**Secret Key:** JBSWY3DPEHPK3PXP

You can use this account to test 2FA by:
1. Logging in with the test account
2. Using the provided secret key in Google Authenticator
3. Testing recovery codes (if needed)

---

## How It Works (Technical)

1. **Setup:** Your secret key is generated and stored in the database
2. **QR Code:** Encodes the secret key + email in otpauth:// URI format
3. **Verification:** Your authenticator app generates 6-digit codes using RFC 6238 TOTP algorithm
4. **Login:** You enter the code, server verifies it matches within a 30-second window
5. **Disable:** Secret key and recovery codes are deleted from database

---

## Recovery Codes Format

Example:
```
ABCD-EFGH
IJKL-MNOP
QRST-UVWX
...
(10 codes total)
```

Each code:
- 8 characters (4-4 format)
- One-time use only
- Case-insensitive
- Can include numbers and letters (excluding confusing ones like 0, O, 1, I)

