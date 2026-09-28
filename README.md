# Wudil Food — ASP.NET Core MVC marketplace

A responsive first-release marketplace for Wudil and ADUSTECH, built with ASP.NET Core MVC, Identity, EF Core and PostgreSQL.

## Run locally

Requirements: .NET 8 SDK and PostgreSQL 16 or later running locally.

```bash
# Edit appsettings.json or override ConnectionStrings__DefaultConnection.
# Set a strong bootstrap password; the first launch creates an administrator.
export BootstrapAdmin__Email=admin@example.com
export BootstrapAdmin__Password='replace-with-a-long-secret'
dotnet restore
dotnet run
```

Open the HTTPS URL printed by `dotnet run`. The application creates the initial schema and reference data on first launch with `EnsureCreated`; for production, generate EF Core migrations and switch to `Migrate` before deploying or evolving the schema. Never put production credentials in source control. Set `ASPNETCORE_ENVIRONMENT=Production`, use HTTPS, persistent PostgreSQL storage, and a persistent ASP.NET Data Protection key ring in production.

## Functional flows

- Customers may register or check out as guests, browse vendors and meals, filter by food/vendor/category/location/price/day, add quantities to a single-vendor cart, select a future slot and fulfilment/payment options, receive an order reference, confirm receipt, review completed orders, or open a complaint.
- Vendors publish immediately, manage their profile, item stock and a weekly preorder slot, and advance valid orders through fulfilment stages.
- Administrators may suspend vendors, record payment status, moderate reviews, resolve complaints, configure delivery fee/coverage, add categories, and view counts and popular items.
- Checkout rechecks price, stock, vendor status and schedule in a serializable database transaction and snapshots line prices in the order.

## Supervisor decisions to confirm

1. **Database fields:** Are business registration, exact pickup points, customer phone verification, dietary tags, order-level tax and item-level event quantities needed?
2. **Permissions:** Can vendors record payment or cancel after preparation? This implementation reserves payment changes for administrators.
3. **Notifications:** This release uses in-platform notices only. Confirm whether email, SMS or WhatsApp and their providers are required.
4. **Delivery:** The administrator sets a flat fee and textual coverage; address coverage is not geocoded or automatically enforced. Confirm zones, distance fees and fulfilment responsibility.
5. **Payment:** Cash, bank transfer and pay-on-collection are records, not transaction processing. Bank transfer starts pending verification. Confirm beneficiary details, proof, reconciliation and refund process before launch.
6. **Scheduling:** The UI supports one weekly preorder slot per item. Confirm multiple slots, holiday closures, per-slot inventory and cutoff interpretation. Slot entry uses server local time; set the server timezone to Africa/Lagos for Nigeria deployment.

## Known first-release boundaries

The code focuses on the ordering cycle. Images are external URLs; uploads, automatic online payment, SMS, geocoding, vendor removal, customer account administration, pagination, detailed vendor/date reporting, robust cancellation inventory restoration, deployment manifests and automated tests require further work. `EnsureCreated` is for initial setup, not schema upgrades. Runtime compilation was unavailable in the authoring environment; run `dotnet build` locally before deployment.
