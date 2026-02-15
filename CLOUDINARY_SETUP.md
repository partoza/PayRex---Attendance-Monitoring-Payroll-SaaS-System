# Cloudinary Setup Guide for PayRex

## Overview
PayRex uses Cloudinary for profile picture storage. This guide walks you through setting up Cloudinary for production use.

## 1. Create a Cloudinary Account

1. Go to [https://cloudinary.com/users/register_free](https://cloudinary.com/users/register_free)
2. Sign up for a free account (Free tier includes 25 credits/month)
3. Verify your email address

## 2. Get Your API Credentials

1. Log in to your Cloudinary dashboard
2. Navigate to **Settings** ? **API Keys** (or find them on the main dashboard)
3. Copy the following values:
   - **Cloud Name**: Your unique cloud identifier
   - **API Key**: Your public API key
   - **API Secret**: Your secret key (keep this confidential!)

## 3. Configure PayRex API

### Development (appsettings.Development.json)

```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

### Production (Use Environment Variables or Azure Key Vault)

**Option A: Environment Variables**
```bash
CLOUDINARY__CLOUDNAME=your-cloud-name
CLOUDINARY__APIKEY=your-api-key
CLOUDINARY__APISECRET=your-api-secret
```

**Option B: Azure Key Vault**
Store secrets in Azure Key Vault and reference them in your configuration.

**Option C: User Secrets (Development Only)**
```bash
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "your-api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your-api-secret"
```

## 4. Create an Upload Preset (Optional but Recommended)

1. In Cloudinary dashboard, go to **Settings** ? **Upload**
2. Scroll to **Upload presets** section
3. Click **Add upload preset**
4. Configure:
   - **Preset name**: `payrex_profiles`
   - **Signing mode**: `Signed` (for security)
   - **Folder**: `payrex/profiles`
   - **Allowed formats**: `jpg, png, webp`
   - **Max file size**: `2000000` (2MB)
   - **Transformation**: 
     - Width: 400
     - Height: 400
     - Crop: fill
     - Gravity: face

## 5. Security Best Practices

### Never Expose Secrets in Frontend
- API Secret should NEVER be sent to the browser
- All uploads go through your backend API
- The Cloudinary service runs server-side only

### Use HTTPS Only
The CloudinaryService is configured to always use HTTPS:
```csharp
_cloudinary = new Cloudinary(account)
{
    Api = { Secure = true }
};
```

### Restrict Upload Types
The service only accepts:
- JPG/JPEG
- PNG
- WEBP

Maximum file size: 2MB

### Image Transformations
Uploaded images are automatically:
- Resized to 400x400
- Cropped to fill with face detection
- Optimized for quality
- Converted to best format

## 6. API Endpoints

### Upload Profile Image
```
POST /api/profile/image
Content-Type: multipart/form-data
Authorization: Bearer {token}

Body: file (image file)
```

### Remove Profile Image
```
DELETE /api/profile/image
Authorization: Bearer {token}
```

## 7. Troubleshooting

### "Cloudinary is not configured" Warning
This appears when credentials are missing. Ensure all three values are set:
- CloudName
- ApiKey
- ApiSecret

### "Invalid file type" Error
Only JPG, PNG, and WEBP files are accepted. Check the Content-Type header.

### "File size exceeds limit" Error
Maximum file size is 2MB. Compress or resize the image before uploading.

### Upload Fails Silently
Check the application logs for detailed error messages:
```
[Error] Cloudinary upload failed: {error message}
```

## 8. Folder Structure in Cloudinary

All profile images are stored in:
```
payrex/
??? profiles/
    ??? user_1_20240101120000.jpg
    ??? user_2_20240102130000.png
    ??? saas_1_20240103140000.webp
```

The naming convention is:
- `user_{userId}_{timestamp}` for company users
- `saas_{userId}_{timestamp}` for SaaS admin users

## 9. Costs and Limits

### Free Tier (25 Credits/Month)
- Storage: 25 GB
- Monthly bandwidth: 25 GB
- Transformations: 25,000

### Usage Estimates
- Profile image upload: ~0.001 credit
- Image transformation: ~0.001 credit
- Image delivery: Based on bandwidth

For a small-medium business, the free tier should be sufficient for profile pictures.

## 10. Migrating Existing Images

If you have existing profile images stored elsewhere:

1. Export current image URLs
2. Use Cloudinary's fetch feature or upload via API
3. Update database `ProfileImageUrl` columns with new Cloudinary URLs

---

## Quick Start Checklist

- [ ] Create Cloudinary account
- [ ] Copy Cloud Name, API Key, and API Secret
- [ ] Add credentials to `appsettings.json` (development) or environment variables (production)
- [ ] Run database migration: `dotnet ef database update`
- [ ] Restart the API application
- [ ] Test upload via Profile Settings page
