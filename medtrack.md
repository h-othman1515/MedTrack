**ORANGE INTERNSHIP - PROJECT DOCUMENTATION**

_Full Technical & Business Proposal_

**MedTrack Jordan**

_Medication Expiry & Shortage Tracker for Pharmacies_

Built with ASP.NET Core 8 | Jordan | 2025

Table of Contents

[1\. Executive Summary 2](#_Toc229654470)

[2\. Problem Statement 2](#_Toc229654471)

[2.1 The Pharmaceutical Supply Chain Problem in Jordan 2](#_Toc229654472)

[2.2 Stakeholder Pain Summary 2](#_Toc229654473)

[3\. Proposed Solution 3](#_Toc229654474)

[3.1 Inventory Management Module 3](#_Toc229654475)

[3.2 Expiry Alert Engine 3](#_Toc229654476)

[3.3 Surplus Sharing Marketplace 3](#_Toc229654477)

[3.4 National Analytics Dashboard (MOH) 3](#_Toc229654478)

[3.5 Distributor Restock Portal 4](#_Toc229654479)

[4\. System Architecture 4](#_Toc229654480)

[4.1 Architecture Overview 4](#_Toc229654481)

[4.2 Database Entity Design 4](#_Toc229654482)

[5\. Technology Stack 5](#_Toc229654483)

[6\. Core Features - Detailed Breakdown 6](#_Toc229654484)

[6.1 Role-Based Access Control 6](#_Toc229654485)

[6.2 Expiry Alert Logic 6](#_Toc229654486)

[6.3 Shortage Detection Algorithm 6](#_Toc229654487)

[6.4 Surplus Matching 6](#_Toc229654488)

[7\. Project Plan & Timeline 7](#_Toc229654489)

[8.1 Why This Is a Real Business Opportunity 8](#_Toc229654490)

[8.2 Revenue Streams 8](#_Toc229654491)

[8.3 Orange Jordan's Strategic Advantage 8](#_Toc229654492)

[SMS Infrastructure Revenue 8](#_Toc229654493)

[IoT and Connectivity Expansion 9](#_Toc229654494)

[Healthcare Sector Market Entry 9](#_Toc229654495)

[Brand and CSR Value 9](#_Toc229654496)

[8.4 Competitive Moat 9](#_Toc229654497)

[8.5 Go-To-Market Strategy 9](#_Toc229654498)

[Phase 1 - Pilot (Months 1-3) 9](#_Toc229654499)

[Phase 2 - MOH Partnership (Months 4-6) 10](#_Toc229654500)

[Phase 3 - National Rollout (Months 7-12) 10](#_Toc229654501)

[8.6 Financial Projection (Conservative, Excluding Grants) 10](#_Toc229654502)

[9\. Risks & Mitigation 10](#_Toc229654503)

[10\. Conclusion 11](#_Toc229654504)

# 1\. Executive Summary

MedTrack Jordan is a web-based platform that addresses a critical gap in Jordan's pharmaceutical supply chain: the complete absence of real-time inventory visibility between pharmacies, distributors, and the Ministry of Health (MOH).

Jordan imports over 90% of its medications, making supply chain fragility a genuine public health risk. Pharmacies operate in silos - over-ordering costly drugs that expire unused, while neighboring pharmacies face shortages of the same medications. There is no shared system, no early warning mechanism, and no national dashboard.

MedTrack Jordan solves this with three interconnected capabilities: smart inventory management with automated expiry alerts, a peer-to-peer surplus sharing marketplace between pharmacies, and a national shortage analytics dashboard for MOH decision-makers.

The platform is built on ASP.NET Core 8, demonstrating full-stack enterprise development skills. It is designed for real-world deployment and carries multiple viable revenue streams, making it both a strong internship project and a genuine business opportunity.

# 2\. Problem Statement

## 2.1 The Pharmaceutical Supply Chain Problem in Jordan

Jordan's healthcare system depends on a fragile import-heavy medication supply. Several structural failures compound the problem:

- Pharmacies have no shared visibility into each other's stock levels, making surplus-sharing impossible without phone calls or personal connections.
- Expiry waste is significant - pharmacies routinely discard expired medications worth hundreds of dinars monthly because no system prompted early action.
- Shortage detection is reactive, not proactive. The MOH learns about regional drug shortages only after patients and doctors report them - days or weeks after the problem began.
- Distributors receive restock requests inconsistently, often via WhatsApp or phone, with no structured demand data to plan shipments.
- Critical medications - insulin, blood pressure drugs, antibiotics - are at the highest risk. A shortage of these directly endangers lives.

## 2.2 Stakeholder Pain Summary

| **Stakeholder**      | **Current Pain Point**                                | **Consequence**                         |
| -------------------- | ----------------------------------------------------- | --------------------------------------- |
| Community Pharmacist | No expiry alerts; discovers waste only at stock count | Direct financial loss                   |
| Hospital Pharmacy    | Cannot find surplus stock from nearby pharmacies      | Patient care delayed                    |
| Drug Distributor     | No structured demand signals; ships on guesswork      | Overstock in some areas, gaps elsewhere |
| Ministry of Health   | No live dashboard of national medication availability | Policy decisions made on outdated data  |
| Patient              | Pharmacy says we don't have it, try elsewhere         | Delayed or missed treatment             |

# 3\. Proposed Solution

MedTrack Jordan is a three-sided platform serving pharmacies, health authorities, and distributors through four core modules.

## 3.1 Inventory Management Module

Pharmacies log their stock with medication name, quantity, batch number, and expiry date. The system maintains a live color-coded dashboard: green items are healthy, amber items expire within 60 days, and red items expire within 30 days or are already expired. Pharmacists can bulk-import stock via CSV for efficiency.

## 3.2 Expiry Alert Engine

Automated notifications trigger at 60-day, 30-day, and 7-day expiry thresholds. Each alert includes a suggested action - donate to a nearby clinic, post on the surplus board, or contact the distributor for return. Alerts are delivered via SMS (Orange network), email, and in-app notifications, ensuring pharmacists are reached regardless of whether they are logged in.

## 3.3 Surplus Sharing Marketplace

Pharmacies with excess stock post surplus listings including medication name, quantity, expiry date, condition, and their governorate. Pharmacies in need browse listings filtered by proximity and medication type. Transfer requests are made through the platform, and both parties confirm completion. Every transfer is logged for audit and MOH reporting purposes.

## 3.4 National Analytics Dashboard (MOH)

A restricted dashboard for Ministry of Health staff showing shortage hotspots by governorate on a map, the top 20 most frequently short medications nationwide, expiry waste volumes by pharmacy and region, transfer activity data, and trend lines comparing current month to previous months.

## 3.5 Distributor Restock Portal

When a pharmacy's stock of any medication drops below its configured minimum threshold, the system automatically generates a restock request to the linked distributor. Distributors see an aggregated demand view showing how many pharmacies in each governorate are below minimum for each drug - enabling smarter shipment planning.

# 4\. System Architecture

## 4.1 Architecture Overview

MedTrack Jordan uses a clean N-Tier architecture with four layers: Presentation, Application Services, Domain/Business Logic, and Data Access. This separation ensures maintainability and testability, and allows individual components to be upgraded or replaced independently.

| **Layer**       | **Technology**                               | **Responsibility**                              |
| --------------- | -------------------------------------------- | ----------------------------------------------- |
| Presentation    | ASP.NET Core MVC + Razor Pages               | Server-rendered UI, role-based views            |
| REST API        | ASP.NET Core Web API                         | JSON endpoints for AJAX and future mobile app   |
| Business Logic  | C# Service Classes + Domain Models           | Alert rules, matching logic, shortage detection |
| Data Access     | Entity Framework Core 8 + Repository Pattern | DB queries, migrations, transactions            |
| Database        | SQL Server 2022                              | Primary relational data store                   |
| Background Jobs | Hosted Services (IHostedService)             | Daily expiry scan, alert dispatch               |
| Notifications   | Twilio SMS + SendGrid Email                  | Multi-channel alert delivery                    |
| Authentication  | ASP.NET Core Identity + JWT                  | Login, roles, API security                      |
| Caching         | IMemoryCache / Redis                         | Dashboard queries, frequent lookups             |
| Maps            | Leaflet.js (client-side)                     | Governorate shortage heatmap                    |

## 4.2 Database Entity Design

| **Entity**      | **Key Fields**                                               | **Relationships**                            |
| --------------- | ------------------------------------------------------------ | -------------------------------------------- |
| Pharmacy        | Id, Name, LicenseNo, Governorate, ContactEmail               | Has many: MedicationBatches, SurplusPosts    |
| MedicationBatch | Id, PharmacyId, DrugName, Quantity, BatchNo, ExpiryDate      | Belongs to: Pharmacy; Has many: ExpiryAlerts |
| ExpiryAlert     | Id, BatchId, AlertLevel (60/30/7), SentAt, Channel           | Belongs to: MedicationBatch                  |
| SurplusPost     | Id, PharmacyId, DrugName, Quantity, ExpiryDate, Status       | Has many: TransferRequests                   |
| TransferRequest | Id, SurplusPostId, RequestingPharmacyId, Status, CompletedAt | Belongs to: SurplusPost and Pharmacy         |
| Drug            | Id, GenericName, Category, MinStockLevel                     | Referenced by MedicationBatch                |
| Notification    | Id, PharmacyId, Type, Channel, Message, SentAt               | Audit log for all alerts sent                |

# 5\. Technology Stack

| **Category**     | **Tool**                  | **Version** | **Purpose**                        |
| ---------------- | ------------------------- | ----------- | ---------------------------------- |
| Web Framework    | ASP.NET Core              | 8.0 LTS     | Main application framework         |
| Language         | C#                        | 12          | Primary development language       |
| ORM              | Entity Framework Core     | 8.x         | Database access and migrations     |
| Database         | SQL Server                | 2022        | Relational data persistence        |
| Authentication   | ASP.NET Core Identity     | 8.x         | User accounts and role management  |
| API Security     | JWT Bearer Tokens         | \-          | Secure API endpoint access         |
| Background Tasks | IHostedService            | \-          | Scheduled expiry scans and alerts  |
| SMS Alerts       | Twilio SDK                | 7.x         | Orange-routed SMS notifications    |
| Email Alerts     | SendGrid                  | 9.x         | HTML email alert delivery          |
| Frontend UI      | Razor Pages + Bootstrap 5 | 5.3         | Responsive server-rendered UI      |
| Charts           | Chart.js                  | 4.x         | Analytics dashboard visualizations |
| Maps             | Leaflet.js                | 1.9         | Governorate shortage heatmap       |
| API Docs         | Swagger (Swashbuckle)     | 6.x         | Interactive API documentation      |
| File Import      | CsvHelper                 | 31.x        | Bulk stock CSV import              |
| Testing          | xUnit + Moq               | \-          | Unit and integration testing       |
| Deployment       | Azure App Service or IIS  | \-          | Hosting and deployment target      |

# 6\. Core Features - Detailed Breakdown

## 6.1 Role-Based Access Control

| **Role**         | **Access Level**        | **Key Capabilities**                                            |
| ---------------- | ----------------------- | --------------------------------------------------------------- |
| Pharmacy Staff   | Own pharmacy data only  | Log stock, view alerts, browse surplus board, request transfers |
| Pharmacy Manager | Own pharmacy + reports  | All staff actions + configure thresholds + view waste reports   |
| Distributor      | Restock requests only   | View aggregated demand by region, confirm deliveries            |
| MOH Admin        | Read-only national view | National analytics dashboard, export reports, shortage alerts   |

## 6.2 Expiry Alert Logic

A background Hosted Service runs nightly and scans all MedicationBatch records. For each batch approaching expiry, it checks whether an alert has already been sent at that threshold level. If not, it creates an ExpiryAlert record, queues the notification, and dispatches it via configured channels. The pharmacy can acknowledge the alert and log what action they took - donation, disposal, or surplus posting.

## 6.3 Shortage Detection Algorithm

Every time a pharmacy updates stock for a medication, the system checks how many pharmacies in the same governorate currently have that drug below minimum threshold. If three or more pharmacies in the same governorate are simultaneously below minimum for the same drug, the system creates a regional shortage flag and sends an alert to the MOH dashboard. This threshold is configurable per drug category.

## 6.4 Surplus Matching

When a pharmacy searches for surplus stock of a needed medication, results are sorted by: (1) distance in same governorate first, (2) nearest expiry date shown first to encourage use of stock that would otherwise waste, (3) quantity available. Transfers are coordinated offline and both parties confirm completion in the system.

# 7\. Project Plan & Timeline

| **Phase**                      | **Weeks** | **Key Deliverables**                                                                            |
| ------------------------------ | --------- | ----------------------------------------------------------------------------------------------- |
| Phase 1 - Setup & Auth         | 1-2       | Project scaffold, SQL Server setup, Identity auth, role-based login, pharmacy registration flow |
| Phase 2 - Inventory Module     | 3-4       | MedicationBatch CRUD, CSV import, color-coded dashboard, minimum threshold configuration        |
| Phase 3 - Alert Engine         | 5-6       | IHostedService background job, 60/30/7-day logic, Twilio SMS integration, SendGrid email        |
| Phase 4 - Surplus Marketplace  | 7-8       | SurplusPost CRUD, TransferRequest workflow, proximity sorting, transfer audit log               |
| Phase 5 - Analytics Dashboard  | 9-10      | MOH dashboard, Chart.js visualizations, Leaflet.js shortage map, shortage detection algorithm   |
| Phase 6 - Testing & Deployment | 11-12     | xUnit tests, bug fixes, Swagger docs, Azure or IIS deployment, demo walkthrough                 |

**8\. Business Advantage & Commercial Strategy**

## 8.1 Why This Is a Real Business Opportunity

MedTrack Jordan sits at the intersection of three trends that make it commercially viable right now:

- Jordan's government is actively digitizing healthcare infrastructure under the Digital Jordan 2025 initiative, creating demand for exactly this kind of data platform.
- International health donors (WHO, USAID, GIZ) regularly fund pharmaceutical supply chain transparency projects in the MENA region with grants of \$200,000-\$2,000,000.
- No competing product currently serves the Jordanian pharmacy market with these specific capabilities - the closest alternatives are generic ERP systems not built for this use case.

## 8.2 Revenue Streams

| **Revenue Stream**         | **Target Customer**                | **Model**                            | **Est. Annual Value**     |
| -------------------------- | ---------------------------------- | ------------------------------------ | ------------------------- |
| MOH National License       | Ministry of Health Jordan          | Annual SaaS license                  | \$40,000-\$80,000/yr      |
| Distributor Intelligence   | Drug distributors (3-5 in Jordan)  | Monthly subscription per distributor | \$6,000-\$15,000/yr       |
| Premium Pharmacy Tier      | Large hospital & chain pharmacies  | Freemium - advanced reports + API    | \$500-\$1,200/pharmacy/yr |
| SMS Notification Volume    | All pharmacies on platform         | Per-SMS fee via Orange network       | Revenue share with Orange |
| WHO/USAID Grant            | International health organizations | Project grant for deployment + scale | \$200k-\$500k one-time    |
| Regional Expansion License | MOH in Iraq, Lebanon, Egypt        | White-label SaaS per country         | \$30k-\$60k/country/yr    |

## 8.3 Orange Jordan's Strategic Advantage

### SMS Infrastructure Revenue

Every expiry alert, transfer notification, and restock request sent through MedTrack Jordan can be routed via Orange's SMS API. With 500 pharmacies sending an average of 10 alerts per month, that is 5,000 SMS messages per month - a recurring, growing revenue stream for Orange's enterprise messaging division.

### IoT and Connectivity Expansion

Phase 2 of MedTrack can integrate with smart refrigerator temperature sensors and connected medicine cabinets - all of which require SIM cards and data connectivity. Orange sells IoT SIM cards and connectivity packages. MedTrack creates direct demand for these products in the healthcare sector.

### Healthcare Sector Market Entry

Orange Jordan currently has limited presence in the healthcare sector beyond basic connectivity. MedTrack gives Orange a credible, data-driven entry point into healthcare digital transformation - a market segment that governments and international donors are actively funding across the MENA region.

### Brand and CSR Value

A publicly visible platform that prevents medication waste and reduces drug shortages generates significant positive media and government goodwill for Orange Jordan. This aligns with Orange Group's global Orange for Good CSR program and can be highlighted in annual sustainability reports.

## 8.4 Competitive Moat

Once pharmacies log their inventory in MedTrack, switching costs are high - their historical data, audit logs, and established transfer relationships are all inside the platform. The MOH's reliance on the national dashboard creates an institutional dependency that makes the platform sticky at the government level.

| **Advantage Factor**  | **Description**                                                          | **Strength** |
| --------------------- | ------------------------------------------------------------------------ | ------------ |
| First-mover in Jordan | No comparable product exists in the Jordanian market today               | High         |
| Data network effect   | More pharmacies = better shortage detection = more value for everyone    | High         |
| Switching cost        | Historical inventory data and audit logs keep pharmacies on the platform | High         |
| Government dependency | MOH dashboard creates institutional reliance on platform continuity      | High         |
| Grant fundability     | International health donors actively seek this type of project           | Medium-High  |
| Orange synergy        | SMS, IoT, and enterprise sales align with existing Orange product lines  | High         |

## 8.5 Go-To-Market Strategy

### Phase 1 - Pilot (Months 1-3)

Launch with 10-20 pharmacies in one governorate (recommended: Amman East). Partner with the Jordan Pharmacists Association for credibility. Offer the service free to all pilot participants in exchange for feedback and testimonials.

### Phase 2 - MOH Partnership (Months 4-6)

Present the shortage detection data collected during the pilot to the MOH. The data itself is the sales pitch - real shortage patterns, real waste numbers. Negotiate a national license agreement.

### Phase 3 - National Rollout (Months 7-12)

Expand to all 12 governorates. Onboard distributors. Begin charging the premium tier to large pharmacies. Apply for WHO/USAID grant funding using the MOH partnership as validation.

## 8.6 Financial Projection (Conservative, Excluding Grants)

| **Year** | **Pharmacies**      | **MOH License** | **Distributor Fees** | **SMS Revenue** | **Total Revenue** |
| -------- | ------------------- | --------------- | -------------------- | --------------- | ----------------- |
| Year 1   | 150                 | \$40,000        | \$6,000              | \$3,600         | ~\$50,000         |
| Year 2   | 500                 | \$60,000        | \$12,000             | \$12,000        | ~\$85,000         |
| Year 3   | 1,000 + 2 countries | \$80,000        | \$30,000             | \$24,000        | ~\$135,000+       |

These figures exclude grant funding - a single WHO/USAID grant of \$200,000-\$500,000 would fund full development and initial deployment and is a realistic milestone given the MOH partnership pathway described above.

# 9\. Risks & Mitigation

| **Risk**                       | **Likelihood** | **Impact** | **Mitigation Strategy**                                                                               |
| ------------------------------ | -------------- | ---------- | ----------------------------------------------------------------------------------------------------- |
| Low pharmacy adoption          | Medium         | High       | Partner with Jordan Pharmacists Association; offer free tier; simplify onboarding to under 10 minutes |
| MOH data sharing concerns      | Medium         | High       | Aggregate-only data for MOH view; no patient data collected; full PDPL compliance                     |
| SMS delivery failures          | Low            | Medium     | Multi-channel fallback (in-app + email if SMS fails); delivery receipt tracking                       |
| Competitor enters market       | Low            | Medium     | First-mover advantage and data network effects make catch-up very difficult after 12 months           |
| Scope too large for internship | Medium         | Medium     | Core MVP is inventory and alerts only; surplus board and MOH dashboard are Phase 2 extensions         |

# 10\. Conclusion

MedTrack Jordan is a technically rigorous, commercially viable project that solves a documented and urgent problem in Jordan's healthcare supply chain. It demonstrates mastery of the full ASP.NET Core stack - from EF Core migrations and background services to REST APIs, role-based auth, and data visualization - while delivering genuine value to pharmacists, the Ministry of Health, and drug distributors.

From a business perspective, it is uniquely positioned: no direct competition exists in Jordan today, multiple revenue streams are available from day one, international grant funding is a realistic and well-trodden path, and it creates direct synergy with Orange Jordan's SMS, IoT, and enterprise connectivity product lines.

Built as an internship prototype, MedTrack Jordan has a credible and realistic path to becoming a production-grade SaaS platform serving Jordan's 3,500+ registered pharmacies - and ultimately expanding across the MENA region.

_MedTrack Jordan | Orange Internship 2025 | Built with ASP.NET Core 8_