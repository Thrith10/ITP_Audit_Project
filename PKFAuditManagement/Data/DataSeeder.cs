using Elfie.Serialization;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Models;
using System.Drawing;
using System.IO;
using System.Numerics;
using System;

namespace PKFAuditManagement.Data
{
    public class DataSeeder
    {
        private readonly ModelBuilder modelBuilder;

        public DataSeeder(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }

        public void Seed()
        {
            modelBuilder.Entity<QC6SubForm>().HasData(
                   new QC6SubForm() { QC6SubFormID = 1, SubFormType = "NEW ENGAGEMENT" },
                   new QC6SubForm() { QC6SubFormID = 2, SubFormType = "AUDIT AND REVIEW ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" },
                   new QC6SubForm() { QC6SubFormID = 3, SubFormType = "NON-ASSURANCE ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" }
            );

            modelBuilder.Entity<QC6FormObjective>().HasData(
                   // Seeding for QC6 First Sub Form
                   new QC6FormObjective() { QC6FormObjectiveID = 1, QC6SubFormID = 1, ObjectiveNo = 1, Objective = "Objective – To ensure that the work for the client does not involve unacceptable risks" },
                   new QC6FormObjective() { QC6FormObjectiveID = 2, QC6SubFormID = 1, ObjectiveNo = 2, Objective = "Objective – To ensure that the firm can comply with relevant ethical requirements" },
                   new QC6FormObjective() { QC6FormObjectiveID = 3, QC6SubFormID = 1, ObjectiveNo = 3, Objective = "Objective - To ensure that the firm has the ability to perform the professional services properly" },
                   new QC6FormObjective() { QC6FormObjectiveID = 4, QC6SubFormID = 1, ObjectiveNo = 4, Objective = "Objective - To ensure that our appointment is properly effected and that the scope of our work is acknowledged by the client" },
                   new QC6FormObjective() { QC6FormObjectiveID = 5, QC6SubFormID = 1, ObjectiveNo = 5, Objective = "Objective – To ensure that the entity has been properly constituted in compliance with relevant legislation." },
                   new QC6FormObjective() { QC6FormObjectiveID = 6, QC6SubFormID = 1, ObjectiveNo = 6, Objective = "Objective – To establish the identity of the client to assist in identifying possible instances of money laundering (“AML”) and terrorism financing activities (“CTF”)." },
                   // Seeding for QC6 Second Sub Form
                   new QC6FormObjective() { QC6FormObjectiveID = 7, QC6SubFormID = 2, ObjectiveNo = 7, Objective = "Objective – Acceptance Procedures" },
                   // Seeding for QC6 Third Sub Form
                   new QC6FormObjective() { QC6FormObjectiveID = 8, QC6SubFormID = 3, ObjectiveNo = 8, Objective = "Objective – Acceptance Procedures" }
            );

            modelBuilder.Entity<QC6FormTestDescription>().HasData(
                   // Seeding for QC6 Sub Form Test Descriptions
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 1,
                       QC6FormObjectiveID = 1,
                       DescriptionNo = 1,
                       Description = "<p>1. Decide whether we can rely on the management's integrity and whether association with the client may damage the firm's reputation. Consider:</p><ul><li>discussion with the party who introduced the client;</li><li>knowledge gained through work done by other departments within the firm;</li><li>media and any other reports;</li><li>client's relationship with any regulatory authority; and</li><li>client's attitude to fairness in financial reporting.</li></ul>"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 2,
                       QC6FormObjectiveID = 1,
                       DescriptionNo = 2,
                       Description = "<p>2. If we have doubts resulting from the considerations above, decide whether the potential risks can be mitigated by introducing additional precautions, such as:</p><ul><li>additional procedures, especially independent third-party confirmations; or</li><li>additional review by an engagement quality reviewer (i.e. safeguard reviewer).</li></ul>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 3,
                       QC6FormObjectiveID = 2,
                       DescriptionNo = 3,
                       Description = "<p>3. Consider whether the firm can comply with relevant ethical requirements, including the firm’s independence. Include in your consideration potential independence or conflicts of interest problems arising from relationships with other clients, firm members, or their families; providing non-assurance services; or unpaid fees.</p>\r\n<p>4. Regarding the proposed relationship between the firm and the client or any associated companies, decide whether we are, and can be seen to be, independent of the client.  Consider:</p><ul><li>Undue dependence</li><li>Proposed contingent fees</li><li>Preparation of accounting records</li><li>Receipt of hospitality, goods, or services</li><li>Voting on audit appointments</li><li>Director or senior employee joining client</li><li>Participation in affairs of client</li><li>Influences from the outside practice (including associated firms)</li><li>Litigation (actual or threatened)</li><li>Family or professional relationships</li><li>Prohibited services</li><li>Financial involvement, including trustee investments, mutual business interests, loans, beneficial interests- shares/trusts, overdue fees</li><li>Network form independence conflict checking procedures performed (i.e. conflict of interest to be considered by completing either “Audit and Review Engagements – Firm and Network Independence” or “Non-assurance Engagements – Firm and Network Independence,” which are part of this form.</li></ul>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 4,
                       QC6FormObjectiveID = 3,
                       DescriptionNo = 4,
                       Description = "<p>5. Decide whether we have the competence to carry out the professional services properly. Consider whether special skills are needed to deal with the particular features or specialised reporting requirements of the client.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 5,
                       QC6FormObjectiveID = 3,
                       DescriptionNo = 5,
                       Description = "<p>6. Is there any reason to believe that management will not provide access to all information of which management is aware that is relevant to the preparation of the financial statements including access to information relevant to disclosures?</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 6,
                       QC6FormObjectiveID = 3,
                       DescriptionNo = 6,
                       Description = "<p>7. Confirm that the firm will have adequate resources to be able to do the work properly at the time the client needs it. <br /> Note:  For example, to indicate if the engagement will be performed by outsourced staff.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 7,
                       QC6FormObjectiveID = 4,
                       DescriptionNo = 7,
                       Description = "<p>8. Send, and obtain a response to, a professional enquiry letter to the previous auditors/reporting accountants <b>(if any)</b>.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 8,
                       QC6FormObjectiveID = 4,
                       DescriptionNo = 8,
                       Description = "<p>9. Obtain and review the previous auditor’s letter of resignation <b>(if applicable)</b> and statement of circumstances connected with their leaving office.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 9,
                       QC6FormObjectiveID = 4,
                       DescriptionNo = 9,
                       Description = "<p>10. Obtain a copy of the minutes appointing PKF as auditors.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 10,
                       QC6FormObjectiveID = 4,
                       DescriptionNo = 10,
                       Description = "<p>11. Send an engagement letter and obtain the client’s agreement to its terms before starting work.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 11,
                       QC6FormObjectiveID = 4,
                       DescriptionNo = 11,
                       Description = "<p>12. Carry out other tests necessary to meet this objective and document such additional considerations.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 12,
                       QC6FormObjectiveID = 5,
                       DescriptionNo = 12,
                       Description = "<p>13. Obtain and file documents of constitution and certificate of incorporation <b>(where applicable)</b>.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 13,
                       QC6FormObjectiveID = 5,
                       DescriptionNo = 13,
                       Description = "<p>14. Where possible, verify that appropriate documents have been filed with the relevant authorities and that these support the information provided by the entity’s records.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 14,
                       QC6FormObjectiveID = 5,
                       DescriptionNo = 14,
                       Description = "<p>15. Carry out other tests if necessary to achieve this objective.</p>\r\n"
                   },
                    new QC6FormTestDescription()
                    {
                        QC6FormTestDescriptionID = 15,
                        QC6FormObjectiveID = 6,
                        DescriptionNo = 15,
                        Description = "<p>16. Carry out procedures for establishing/verifying the identity of new clients for possible instances of money laundering if there are indicators or suspicion of such activities based on the evaluation of integrity of the new client set out above. <ol class=\"list-decimal pl-4\"><li>Perform a search of the company’s name, all directors and ultimate beneficial owners owning more than 25% equity interest in the ultimate holding company using LexisNexis. This screening covers not only identifying any PEPs but also terrorist names, terrorist organisations and names sanctioned by United States / United Nations. A copy of the screening should be attached to QC6(3).</li> <br> <li>Carry out a google check on the company’s name, names of the directors and ultimate BOs owning more than 25% equity interest in the ultimate holding company. A copy of the google check should be attached to QC6(3).</li> <br> <li>For audit clients whereby the companies are incorporated by our related entities, PKF-CAP Corporate Services Pte Ltd (“CS”) or PKF-Khoo Management Services Pte Ltd (“KMS”), then carry out a review of the screening and google checking that have been carried out by CS or KMS. If the result is satisfactory, then document the review that you have performed. No further screening or checking is required unless work performed is considered not adequate for our purposes. This placing of reliance is acceptable so long as the screening carried out by CS or KMS is current within 12 months from our client/engagement evaluation date.</li></ol> <i><b>Note:</b> If the prospective client is - (a) an entity listed on the Singapore Exchange; (b) an entity listed on a stock exchange outside Singapore; (c) a Singapore financial institution; (d) a financial institution incorporated or established outside Singapore; or (e) an investment vehicle, the managers of which are — (i) Singapore financial institutions; or (ii) financial institutions incorporated or established outside Singapore, under the Accountants Act 2004, Accountants (Prevention of Money Laundering and Financing of Terrorism) Rules 2023, an accounting entity need not inquire if there exists any beneficial owner unless the accounting entity has doubts about the veracity of the information obtained by the accounting entity in carrying out customer due diligence measures under these Rules or suspects that the client is carrying out or facilitating money laundering or the financing of terrorism. <br><br> “Beneficial owner” means — (a) an individual who ultimately owns all of the assets or undertakings of the client (whether or not the client is a body corporate); (b) an individual who has ultimate control or ultimate effective control over, or has executive authority in, the client; or (c) an individual on whose behalf the client has employed or engaged the services of an accounting entity.</i></p>"
                    },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 16,
                       QC6FormObjectiveID = 6,
                       DescriptionNo = 16,
                       Description = "<p>Are the directors identified to be a Political Exposed Person(“PEP”)? If yes,please establish where the source of funds are obtained?</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 17,
                       QC6FormObjectiveID = 6,
                       DescriptionNo = 17,
                       Description = "<p>Are the ultimate beneficial owners (individuals owning more than 25% equity interest in the ultimate holding company) a PEP? If yes, please establish where the source of funds are obtained?</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 18,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 18,
                       Description = "<p><b>When proposing for an audit or review engagement: </b><br><br>a) Is the potential client listed (public)? If yes, proceed to (c). If not, research the family tree and ascertain if there is a listed (or publicly traded) entity or entities in the group.<br><br>If there are listed entities in the group, proceed to (b). If the potential client is not listed and there are no listed entities in the group, proceed to (d).</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 19,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 19,
                       Description = "<p>b) If there are listed entities in the group, investigate whether the firm or other PKF Member Firms provide audit or review services to these listed entities.<br><br>This investigation shall include direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these listed entities (this is not required).<br><br>If the firm or other Member Firms provide audit or review services to listed entities in the group, proceed to (c). If not, proceed to (d).</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 20,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 20,
                       Description = "<p>c) If the potential client is listed, or the firm or other Member Firms provide audit or review services to listed entities in the group, independence considerations must include all related entities as defined in the IESBA Code for Professional Accountants.<br><br>Enquire as to whether the firm or other Member Firms provide non-assurance services to the potential client or any related entities. This enquiry shall be direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these related entities.<br><br>If the firm or other Member Firms provide non-assurance services to the potential client or any related entities, proceed to (f). If not, no further action is required.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 21,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 21,
                       Description = "<p>d) If the potential client is not listed and there are no listed entities in the group or the firm or other Member Firms do not provide audit or review services to any listed entities in the group, enquire as to whether the firm or other Member Firms provide non-assurance services to the potential client or entities over which the potential client has direct or indirect control.<br><br>This enquiry shall be direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these entities.<br><br>If the firm or other Member Firms provide non-assurance services to the potential client or entities over which the potential client has direct or indirect control, proceed to (f). If not, proceed to (e).</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 22,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 22,
                       Description = "<p>e) Is there reason to believe that a relationship or circumstance involving any other related entity of the potential client is relevant to the evaluation of the firm’s independence from the client? If yes, apply step (f) below to that related entity. If not, no further action is required.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 23,
                       QC6FormObjectiveID = 7,
                       DescriptionNo = 23,
                       Description = "<p>f) Evaluate the significance of the threats identified and apply safeguards when necessary to eliminate the threat or reduce it to an acceptable level. This may include not accepting the engagement.<br><br>Carry out the evaluation under the IESBA International Code of Ethics for Professional Accountants, including International Independence Standards and any local ethical code if it is stricter.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 24,
                       QC6FormObjectiveID = 8,
                       DescriptionNo = 24,
                       Description = "<p><b>When proposing for a non-assurance engagement:</b><br><br>a) Is the potential client listed (public)? If not, research the family tree and ascertain if there is a listed (or publicly traded) entity or entities in the group.<br><br>If the potential client is listed or there are other listed entities in the group, proceed to (b). If the potential client is not listed and there are no listed entities in the group, proceed to (c).</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 25,
                       QC6FormObjectiveID = 8,
                       DescriptionNo = 25,
                       Description = "<p>b) If the potential client is listed or there are other listed entities in the group, investigate whether the firms or other PKF Member Firms provide audit or review services to these listed entities. This investigation shall include direct enquiry of client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these listed entities.<br><br>If the firm or other Member Firms do not provide audit or review services to listed entities in the group, proceed to (c).<br><br>If the firm or other Member Firms provide audit or review services to these listed entities, independence considerations must include all related entities as defined in the IESBA International Code for Professional Accountants, including International Independence Standards. Proceed to (d).</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 26,
                       QC6FormObjectiveID = 8,
                       DescriptionNo = 26,
                       Description = "<p>c) If the potential client is not listed and there are no other listed entities in the group, or the firm or other Member Firms do not provide audit or review services to the listed entities in the group, enquire as to whether the firm or other Member Firms provide audit or review services to the potential client or entities that have direct or indirect control over the potential client.<br><br>This enquiry shall be direct enquiry of the client management and review of the transnational entity listing and may be supplemented by an email to the Member Firm(s) geographically close to these entities.<br><br>If the firm or other Member Firms provide audit or review services to the potential client or entities that have direct or indirect control over the potential client, proceed to (d). If not, no further action is required.</p>\r\n"
                   },
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 27,
                       QC6FormObjectiveID = 8,
                       DescriptionNo = 27,
                       Description = "<p>d) Evaluate the significance of the threats identified and apply safeguards when necessary to eliminate the threat or reduce it to an acceptable level. This may include not accepting the engagement.<br><br>Carry out the evaluation under the IESBA International Code for Professional Accountants, including International Independence Standards and any local ethical code if it is stricter.</p>"
                   }
             );

            modelBuilder.Entity<QC7SubForm>().HasData(
                   new QC7SubForm() { QC7SubFormID = 1, SubFormType = "CONTINUING ENGAGEMENT" },
                   new QC7SubForm() { QC7SubFormID = 2, SubFormType = "AUDIT AND REVIEW ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" },
                   new QC7SubForm() { QC7SubFormID = 3, SubFormType = "NON-ASSURANCE ENGAGEMENTS ONLY - FIRM AND NETWORK INDEPENDENCE (Confirm that no conflict exist. If this section is not applicable, please indicate “NA”)" }
            );

            modelBuilder.Entity<QC7FormObjective>().HasData(
                   // Seeding for QC7 First Sub Form
                   new QC7FormObjective() { QC7FormObjectiveID = 1, QC7SubFormID = 1, ObjectiveNo = 1, Objective = "Objective – To ensure that recurring work for the client does not involve unacceptable risks" },
                   new QC7FormObjective() { QC7FormObjectiveID = 2, QC7SubFormID = 1, ObjectiveNo = 2, Objective = "Objective – To ensure that the firm can comply with relevant ethical requirements" },
                   new QC7FormObjective() { QC7FormObjectiveID = 3, QC7SubFormID = 1, ObjectiveNo = 3, Objective = "Objective - To ensure that the firm has the ability to perform the professional services properly" },
                   new QC7FormObjective() { QC7FormObjectiveID = 4, QC7SubFormID = 1, ObjectiveNo = 4, Objective = "Objective – To ensure that the fees will be adequate and collectible" },
                   new QC7FormObjective() { QC7FormObjectiveID = 5, QC7SubFormID = 1, ObjectiveNo = 5, Objective = "Objective - To ensure that the client's understanding of the scope and terms of the engagement agrees with our own and no events have occurred to adversely affect our relationship with the client" },
                   new QC7FormObjective() { QC7FormObjectiveID = 6, QC7SubFormID = 1, ObjectiveNo = 6, Objective = "Objective - To ensure that the client’s understanding of the scope and terms of the engagement is agreed" },
                   new QC7FormObjective() { QC7FormObjectiveID = 7, QC7SubFormID = 1, ObjectiveNo = 7, Objective = "Objective – To establish the identity of the client to assist in identifying possible instances of money laundering (“AML”) and terrorism financing activities (“CTF”)" },
                   // Seeding for QC7 Second Sub Form
                   new QC7FormObjective() { QC7FormObjectiveID = 8, QC7SubFormID = 2, ObjectiveNo = 8, Objective = "Objective – Continuance Procedures" },
                   // Seeding for QC7 Third Sub Form
                   new QC7FormObjective() { QC7FormObjectiveID = 9, QC7SubFormID = 3, ObjectiveNo = 9, Objective = "Objective – Continuance Procedures" }
            );

            modelBuilder.Entity<QC7FormTestDescription>().HasData(
                   // Seeding for QC7 Sub Form Test Descriptions
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 1,
                       QC7FormObjectiveID = 1,
                       DescriptionNo = 1,
                       Description = "<p>1. Decide whether we can rely on the management's integrity and whether our continued association with the client may damage the firm's reputation. Consider:</p><ul><li>previous year's experience;</li><li>knowledge gained through work done by other service areas within the firm;</li><li>media reports; and</li><li>client's relationship with any regulatory authority.</li></ul>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 2,
                       QC7FormObjectiveID = 1,
                       DescriptionNo = 2,
                       Description = "<p>2. If we have doubts resulting from the considerations above, decide whether the potential risks can be mitigated by introducing additional precautions, such as:</p><ul><li>additional procedures; or</li><li>additional review by an engagement quality reviewer (i.e. safeguard reviewer)</li></ul>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 3,
                       QC7FormObjectiveID = 2,
                       DescriptionNo = 3,
                       Description = "<p>3. Consider whether the firm can comply with relevant ethical requirements, including whether there have been any changes in the firm’s independence. Include in your consideration potential independence or conflicts of interest problems arising from relationships with other clients, firm members, or their families; providing non-assurance services; or unpaid fees.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 4,
                       QC7FormObjectiveID = 2,
                       DescriptionNo = 4,
                       Description = "<p>4. If there has been any change in the relationship between the firm and the client or any associated companies, decide whether we are still, and can be seen still to be, independent of the client. Consider:</p><ul><li>Conflicts of interest</li><li>Undue dependence</li><li>Concerns about losing the engagement</li><li>Contingent fees</li><li>Preparation of accounting records including information obtained from outside of the general and subsidiary ledger</li><li>Receipt of hospitality, goods or services</li><li>Voting on audit appointments</li><li>Director or senior employee joining client</li><li>Participation in affairs of client</li><li>Acting as auditor for a long time</li><li>Influences from outside the practice (including associated firms)</li><li>Litigation (actual or threatened)</li><li>Family or personal relationships</li><li>Provision of other services</li><li>Financial involvement: <ul><li>Trustee investments</li><li>Mutual business interests</li><li>Loans</li><li>Beneficial interests - shares/trusts</li><li>Overdue fees</li><li>Network firm independence threats do not exist </li></ul></li></ul><p>This consideration should include independence in relation to network firms.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 5,
                       QC7FormObjectiveID = 3,
                       DescriptionNo = 5,
                       Description = "<p>5. Decide whether we have the competence to carry out the professional services properly. Consider whether special skills are needed to deal with the particular features or specialised reporting requirements of the client.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 6,
                       QC7FormObjectiveID = 3,
                       DescriptionNo = 6,
                       Description = "<p>6. Confirm that the firm will have adequate resources to be able to do the work properly at the time the client needs it.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 7,
                       QC7FormObjectiveID = 4,
                       DescriptionNo = 7,
                       Description = "<p>7. Decide whether the level of fees agreed will be adequate to allow us to carry out the work deem necessary, properly and with integrity.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 8,
                       QC7FormObjectiveID = 4,
                       DescriptionNo = 8,
                       Description = "<p>8. Consider the client’s ability to pay the agreed fees.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 9,
                       QC7FormObjectiveID = 5,
                       DescriptionNo = 9,
                       Description = "<p>9. Consider whether events have occurred which may affect the firm’s relationship with the client. This should be done if there is:<ul><li>a significant change in management or ownership;</li><li>a change in the client’s legal advisers;</li><li>an adverse change in the client’s financial condition;</li><li>significant litigation against the client by a third party;</li><li>a change in the nature of the business;</li><li>a change in the scope of work to be carried out; or</li><li>a change in the firm’s responsibilities to any regulatory authority.</li></ul>Ensure appropriate procedures are carried out in response to the above matters including a re-evaluation of the firm’s independence, a reconsideration of management’s integrity and a reconsideration of responsibilities in relation to money laundering if relevant.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 10,
                       QC7FormObjectiveID = 6,
                       DescriptionNo = 10,
                       Description = "<p>10. Decide whether we need to renew our engagement letter. If yes, send a revised letter and obtain the client's agreement to its terms before starting work.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 11,
                       QC7FormObjectiveID = 7,
                       DescriptionNo = 11,
                       Description = "<p>11. For anti-money laundering (“AML”) and counter-terrorism financing (“CTF”) activities, please carry out the following procedures:<br><br>&ensp;&ensp;1. Perform a search of the company’s name, all directors and ultimate beneficial owners owning more than 25% equity interest in the ultimate holding company using LexisNexis. This screening covers not only identifying any PEPs but also terrorist names, terrorist organisations and names sanctioned by United States / United Nations. A copy of the screening should be attached to QC7(3).<br><br>&ensp;&ensp;2. Carry out a google check on the company’s name, the names of the directors and ultimate BOs owning more than 25% equity interest in the ultimate holding company. A copy of the google check should be attached to QC7(3).<br><br>&ensp;&ensp;3. For audit clients whereby the companies are incorporated by our related entities, PKF-CAP Corporate Services Pte Ltd (“CS”) or PKF-Khoo Management Services Pte Ltd (“KMS”), then carry out a review of the screening and google checking that have been carried out by CS or KMS. If the result is satisfactory, then document the review that you have performed. No further screening or checking is required unless work performed is considered not adequate for our purposes. This placing of reliance is acceptable so long as the screening carried out by CS or KMS is current within 12 months from our client/engagement evaluation date.<br><br><b><u>Note</u></b><br><br><i>i. <b>Note:</b> If the prospective client is - (a) an entity listed on the Singapore Exchange; (b) an entity listed on a stock exchange outside Singapore; (c) a Singapore financial institution; (d) a financial institution incorporated or established outside Singapore; or (e) an investment vehicle, the managers of which are — (i) Singapore financial institutions; or (ii) financial institutions incorporated or established outside Singapore, under the Accountants Act 2004, Accountants (Prevention of Money Laundering and Financing of Terrorism) Rules 2023, an accounting entity need not inquire if there exists any beneficial owner unless the accounting entity has doubts about the veracity of the information obtained by the accounting entity in carrying out customer due diligence measures under these Rules or suspects that the client is carrying out or facilitating money laundering or the financing of terrorism.<br><br>“Beneficial owner” means — (a) an individual who ultimately owns all of the assets or undertakings of the client (whether or not the client is a body corporate); (b) an individual who has ultimate control or ultimate effective control over, or has executive authority in, the client; or (c) an individual on whose behalf the client has employed or engaged the services of an accounting entity.<br><br>ii. If PEP identified, please refer to (a) any work performed in QC6 and (b) the services we provided in prior year such as audit to ascertain whether there are any suspicious transactions relating to AML or CTF that require us to extend our procedures for these areas.<br><br>iii. For PEPs that are linked to Peoples’ Action Party in Singapore and PEPs where their wealth is known publicly, there is no need to establish their source of funds for their operations.</i></p>"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 12,
                       QC7FormObjectiveID = 8,
                       DescriptionNo = 12,
                       Description = "<p><b>When deciding on whether or not to continue with an audit or review engagement:</b><br><br>Consider changes that may alter the original assessment of the acceptability of the client and engagement, including network firm independence.<br><br><i>Examples of changes that shall cause a careful reconsideration are:<ul><li>a significant change in the size, structure or nature of the client's business;</li><li>a major change in the ownership or management of the client; and</li><li>new regulatory reporting requirements.</li></ul></i><br>Network conflict checking shall require direct enquiry of the client management to determine whether there were any:<ul><li>changes in the controlling ownership interests over the entity;</li><li>changes in any controlling interests held by the entity;</li><li>new listings in the group;</li><li>any known change in service providers that may impact independence; or</li><li>changes in classification of the entity as public interest.</li></ul><br>Any such changes will require reconsideration of the relevant firm and network conflict checking procedures as outlined under acceptance above.</p>\r\n"
                   },
                   new QC7FormTestDescription()
                   {
                       QC7FormTestDescriptionID = 13,
                       QC7FormObjectiveID = 9,
                       DescriptionNo = 13,
                       Description = "<p><b>When deciding on whether or not to continue with an audit or review engagement:</b><br><br>Consider changes that may alter the original assessment of the acceptability of the client and engagement, including network firm independence.<br><br><i>Examples of changes that shall cause a careful reconsideration are:<ul><li>a significant change in the size, structure or nature of the client's business;</li><li>a major change in the ownership or management of the client; and</li><li>new regulatory reporting requirements.</li></ul></i><br>Network conflict checking shall require direct enquiry of the client management to determine whether there were any:<ul><li>changes in the controlling ownership interests over the entity;</li><li>changes in any controlling interests held by the entity;</li><li>new listings in the group;</li><li>any known change in service providers that may impact independence; or</li><li>changes in classification of the entity as public interest.</li></ul><br>Any such changes will require reconsideration of the relevant firm and network conflict checking procedures as outlined under acceptance above.</p>\r\n"
                   }
             );

            modelBuilder.Entity<QC35FormTestDescription>().HasData(
                new QC35FormTestDescription { QC35FormTestDescriptionID = 1, QC35FormID = 1, Description = "No. of working paper files" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 2, QC35FormID = 1, Description = "Working papers are transferred from arch files to paper files" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 3, QC35FormID = 1, Description = "Working paper files are numbered sequentially" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 4, QC35FormID = 1, Description = "All working papers in each file is complete (Manager to initial all working papers)" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 5, QC35FormID = 1, Description = "Date of Audit Report" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 6, QC35FormID = 1, Description = "Date of approval of files for archival is within 60 days from date of Audit Report" },
                new QC35FormTestDescription { QC35FormTestDescriptionID = 7, QC35FormID = 1, Description = "Date of approval and confirmation that CaseWare Audit files has been locked down within 60 days from the date of the Audit Report (if applicable). Refer to the screenshot of CaseWare below." }
            );
             modelBuilder.Entity<QC6Form>().HasData(
                new QC6Form
                {
                    QC6FormID = 1,
                    CreatedBy = "",
                    FileReference = "",
                    ProspectiveClient = "",
                    PeriodEnded = null,
                    EngagementType = "",
                    Industry = "",
                    PreparedBy = "",
                    PreparedByDate = DateTime.MinValue,
                    ReviewedBy = "",
                    Status = "Pending",
                    RejectionReason = null,
                    FormSubmissionDate = DateTime.MinValue,
                    PKFEntityProposingService = "",
                    SourceOfReferral = "",
                    NatureOfServiceForEstimateFee = "",
                    EstimatedFee = 0.00m,
                    BudgetedTimeCost = 0.00m,
                    BudgetedFeeRecoveryRate = 0.00m,
                    OutstandingUnpaidFees = false,
                    AuditFee = 0.00m,
                    GrandTotal = 0.00m,
                    FeeConcentration = 0.00m,
                    ConflictsCheckDone = true,
                    TypeOfActivities = "",
                    ComplexityOfEngagement = "",
                    PredecessorAuditor = false,
                    ReasonsForDiscontinuance = "",
                    PublicInterestEntity = false,
                    PublicInterestEntityType = "",
                    IsSubForm2NotApplicable = false,
                    IsSubForm3NotApplicable = false,
                    IsTemplate = true
                }
                /*new QC6Form
                {
                    QC6FormID = 2,
                    CreatedBy = "User2",
                    FileReference = "FileRef002",
                    ProspectiveClient = "ClientB",
                    PeriodEnded = new DateTime(2023, 12, 31, 23, 59, 59, 999),
                    EngagementType = "Review",
                    PreparedBy = "User3",
                    PreparedByDate = new DateTime(2023, 11, 16, 11, 0, 0, 0),
                    ReviewedBy = "User4",
                    ReviewedByDate = new DateTime(2023, 11, 26, 11, 0, 0, 0),
                    Status = "Approved",
                    RejectionReason = null,
                    FormSubmissionDate = new DateTime(2023, 12, 2, 10, 0, 0, 0),
                    PKFEntityProposingService = "EntityB",
                    SourceOfReferral = "ReferralB",
                    NatureOfServiceForEstimateFee = "Review Services",
                    EstimatedFee = 4000.00m,
                    BudgetedTimeCost = 2500.00m,
                    BudgetedFeeRecoveryRate = 0.65m,
                    OutstandingUnpaidFees = false,
                    AuditFee = 1500.00m,
                    GrandTotal = 5500.00m,
                    FeeConcentration = 0.08m,
                    ConflictsCheckDone = true,
                    TypeOfActivities = "Financial Review",
                    ComplexityOfEngagement = "Medium",
                    PredecessorAuditor = "AuditorB",
                    ReasonsForDiscontinuance = "End of contract",
                    PublicInterestEntity = false,
                    PublicInterestEntityType = null,
                    IsSubForm2NotApplicable = true,
                    IsSubForm3NotApplicable = false
                },
                new QC6Form
                {
                    QC6FormID = 3,
                    CreatedBy = "User3",
                    FileReference = "FileRef003",
                    ProspectiveClient = "ClientC",
                    PeriodEnded = new DateTime(2023, 12, 31, 23, 59, 59, 999),
                    EngagementType = "Compilation",
                    PreparedBy = "User4",
                    PreparedByDate = new DateTime(2023, 11, 17, 12, 0, 0, 0),
                    ReviewedBy = "User5",
                    ReviewedByDate = new DateTime(2023, 11, 27, 12, 0, 0, 0),
                    Status = "Rejected",
                    RejectionReason = "Incomplete data",
                    FormSubmissionDate = new DateTime(2023, 12, 3, 11, 0, 0, 0),
                    PKFEntityProposingService = "EntityC",
                    SourceOfReferral = "ReferralC",
                    NatureOfServiceForEstimateFee = "Compilation Services",
                    EstimatedFee = 3000.00m,
                    BudgetedTimeCost = 2000.00m,
                    BudgetedFeeRecoveryRate = 0.67m,
                    OutstandingUnpaidFees = true,
                    AuditFee = 1000.00m,
                    GrandTotal = 4000.00m,
                    FeeConcentration = 0.06m,
                    ConflictsCheckDone = false,
                    TypeOfActivities = "Financial Compilation",
                    ComplexityOfEngagement = "Low",
                    PredecessorAuditor = "AuditorC",
                    ReasonsForDiscontinuance = "Client merger",
                    PublicInterestEntity = false,
                    PublicInterestEntityType = null,
                    IsSubForm2NotApplicable = true,
                    IsSubForm3NotApplicable = true
                },
                new QC6Form
                {
                    QC6FormID = 4,
                    CreatedBy = "User4",
                    FileReference = "FileRef004",
                    ProspectiveClient = "ClientD",
                    PeriodEnded = new DateTime(2023, 12, 31, 23, 59, 59, 999),
                    EngagementType = "Consulting",
                    PreparedBy = "User5",
                    PreparedByDate = new DateTime(2023, 11, 18, 13, 0, 0, 0),
                    ReviewedBy = "User6",
                    ReviewedByDate = new DateTime(2023, 11, 28, 13, 0, 0, 0),
                    Status = "Submitted",
                    RejectionReason = null,
                    FormSubmissionDate = new DateTime(2023, 12, 4, 12, 0, 0, 0),
                    PKFEntityProposingService = "EntityD",
                    SourceOfReferral = "ReferralD",
                    NatureOfServiceForEstimateFee = "Consulting Services",
                    EstimatedFee = 6000.00m,
                    BudgetedTimeCost = 3500.00m,
                    BudgetedFeeRecoveryRate = 0.58m,
                    OutstandingUnpaidFees = false,
                    AuditFee = 2500.00m,
                    GrandTotal = 8500.00m,
                    FeeConcentration = 0.12m,
                    ConflictsCheckDone = true,
                    TypeOfActivities = "Business Consulting",
                    ComplexityOfEngagement = "Medium",
                    PredecessorAuditor = "AuditorD",
                    ReasonsForDiscontinuance = "Service no longer required",
                    PublicInterestEntity = true,
                    PublicInterestEntityType = "TypeB",
                    IsSubForm2NotApplicable = false,
                    IsSubForm3NotApplicable = true
                },
                new QC6Form
                {
                    QC6FormID = 5,
                    CreatedBy = "User5",
                    FileReference = "FileRef005",
                    ProspectiveClient = "ClientE",
                    PeriodEnded = new DateTime(2023, 12, 31, 23, 59, 59, 999),
                    EngagementType = "Tax",
                    PreparedBy = "User6",
                    PreparedByDate = new DateTime(2023, 11, 19, 14, 0, 0, 0),
                    ReviewedBy = "User7",
                    ReviewedByDate = new DateTime(2023, 11, 29, 14, 0, 0, 0),
                    Status = "Approved",
                    RejectionReason = null,
                    FormSubmissionDate = new DateTime(2023, 12, 5, 13, 0, 0, 0),
                    PKFEntityProposingService = "EntityE",
                    SourceOfReferral = "ReferralE",
                    NatureOfServiceForEstimateFee = "Tax Services",
                    EstimatedFee = 7000.00m,
                    BudgetedTimeCost = 4000.00m,
                    BudgetedFeeRecoveryRate = 0.57m,
                    OutstandingUnpaidFees = true,
                    AuditFee = 3000.00m,
                    GrandTotal = 10000.00m,
                    FeeConcentration = 0.15m,
                    ConflictsCheckDone = true,
                    TypeOfActivities = "Tax Advisory",
                    ComplexityOfEngagement = "High",
                    PredecessorAuditor = "AuditorE",
                    ReasonsForDiscontinuance = "Change in client strategy",
                    PublicInterestEntity = true,
                    PublicInterestEntityType = "TypeC",
                    IsSubForm2NotApplicable = false,
                    IsSubForm3NotApplicable = false
                }*/
            );

            modelBuilder.Entity<QC6FormConclusion>().HasData(
               new QC6FormConclusion
               {
                   QC6FormID = 1,
                   QC6FormConclusionID = 1,
                   AnySignificantRisk = false,
                   NewEngagementRiskRating = "High Risk (H1)",
                   NewEngagementRiskRatingReason = "The Company being a limited liability company incorporated and domiciled in Singapore and is listed on the Main Board of the Stock Exchange of Singapore Limited (the “SGX”) is automatically marked as high risk client.",
                   EngagementSubjectedTo = "Full safeguard review (H1)",
                   SafeguardReviewerAssigned = "",
                   IsNewEngagementAcceptance = "Accepted",
                   IsSuspiciousTransactionReportFiled = false,
                   Satisfaction = "Yes.",
                   PreparedBy = ""
               }
            );

            modelBuilder.Entity<QC6FormTest>().HasData(
               new QC6FormTest
               {
                   QC6FormTestID = 1,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 1,
                   Comments = "We have carried out the following procedures during our evaluation: <br><br>1) Public Record Review - Verified the company profile with the Accounting and Corporate Regulatory Authority (ACRA) business register and found no negative information. <br><br>2) Web Screening - Conducted web searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>3) Database Screening - Conducted Lexis Nexis searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>4) Conflict of Interest Check - Our internal TREND network conflict check confirmed no conflicts of interest.<br><br>5) Internal Conflict of Interest Check - Addressed and cleared any potential conflicts within PKF Singapore through email communication.<br><br>Based on these due diligence procedures, including discussions with management, we have identified no significant issues that could raise concerns regarding the company's integrity or potential reputational risks for PKF Singapore in association with them."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 2,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 2,
                   Comments = "Our due diligence review, including a review of the company profile with ACRA, web and database screening, conflict checks, and discussions with management, identified no significant concerns that would hinder our ability to accept this engagement."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 3,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 3,
                   Comments = "Our due diligence procedures, outlined above, identified no threats to our independence or conflicts of interest with the Company. Based on this assessment, we confirm compliance with all relevant ethical requirements, including those regarding firm independence."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 4,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 4,
                   Comments = "Our team possesses the necessary skills and experience to successfully complete this engagement."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 5,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 5,
                   Comments = "None."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 6,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 6,
                   Comments = "Our team is well-resourced and prepared to handle this engagement effectively."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 7,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 7,
                   Comments = "We have obtained professional clearance from _______."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 8,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 8,
                   Comments = "Not applicable. <br><br> For audit engagement, if applicable: The reasons for the change of auditor were made public in their Company announcement."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 9,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 9,
                   Comments = "Not applicable. <br><br> For audit engagement, if applicable: The reasons for the change of auditor were made public in their Company announcement."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 10,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 10,
                   Comments = "The engagement letter will be prepared and sent to management to be signed upon the approval of QC6(3)."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 11,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 11,
                   Comments = "Having completed our due diligence procedures, we are confident that no further steps are necessary. Our robust acceptance process, as outlined above, ensures we can effectively serve the company."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 12,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 12,
                   Comments = "Our client acceptance evaluation, including a review of the company's ACRA Business Profile, identified no concerns. A detailed review of the Articles and Memorandum of Association will be conducted as part of our standard statutory audit procedures after our appointment is confirmed."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 13,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 13,
                   Comments = "A review of the Articles and Memorandum of Association is a standard part of our statutory audit procedures and will be conducted after our appointment is confirmed."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 14,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 14,
                   Comments = "Not applicable since no exceptions are noted in our procedures performed above."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 15,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 15,
                   Comments = "We have carried out the following procedures during our evaluation:<br><br>1) Public Record Review - Verified the company profile with the Accounting and Corporate Regulatory Authority (ACRA) business register and found no negative information.<br><br>2) Web Screening - Conducted web searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>3) Database Screening - Conducted Lexis Nexis searches on the company name and its directors and authorised representatives, identifying no adverse news or concerns.<br><br>4) Conflict of Interest Check - Our internal TREND network conflict check confirmed no conflicts of interest.<br><br>5) Internal Conflict of Interest Check - Addressed and cleared any potential conflicts within PKF Singapore through email communication.<br><br>Our due diligence process includes a thorough review of public records, web screening, and database checks. This specifically focuses on identifying and screening ultimate beneficial owners with over 25% ownership in the ultimate holding company.  We screen for potential red flags, including Politically Exposed Persons (PEPs), individuals or organisations on terrorist watchlists, and those sanctioned by authorities.<br><br>Based on these thorough due diligence procedures, including discussions with management, we have identified no significant issues that could raise concerns regarding the company's integrity or potential reputational risks for PKF Singapore in association with them."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 16,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 16,
                   Comments = ""
               },
               new QC6FormTest
               {
                   QC6FormTestID = 17,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 17,
                   Comments = "For listed companies: Given the Company's listing on the Stock Exchange of __________, identifying ultimate beneficial owners beyond the information available in their publicly accessible annual report is not necessary.  We recommend referring to the Company's most recent annual report for details on significant shareholders."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 18,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 18,
                   Comments = "Yes."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 19,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 19,
                   Comments = "N/A."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 20,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 20,
                   Comments = "Our due diligence included inquiries with the Company to confirm that neither PKF Singapore nor any other member firm provides non-assurance services to them or any related entities. We also conducted a comprehensive check through our TREND network database to verify this. <br><br> None."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 21,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 21,
                   Comments = "NA."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 22,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 22,
                   Comments = "NA."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 23,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 23,
                   Comments = "Our due diligence procedures identified no threats to our independence or conflicts of interest with the Company or the Group. This assessment confirms our compliance with all relevant ethical requirements, including the International Code of Ethics for Professional Accountants (IESBA Code) and International Independence Standards, along with any stricter local ethical codes."
               },
               new QC6FormTest
               {
                   QC6FormTestID = 24,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 24,
                   Comments = "NA"
               },
               new QC6FormTest
               {
                   QC6FormTestID = 25,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 25,
                   Comments = "NA"
               },
               new QC6FormTest
               {
                   QC6FormTestID = 26,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 26,
                   Comments = "NA"
               },
               new QC6FormTest
               {
                   QC6FormTestID = 27,
                   QC6FormID = 1,
                   QC6FormTestDescriptionID = 27,
                   Comments = "NA"
               }
            );
        }
    }
}
