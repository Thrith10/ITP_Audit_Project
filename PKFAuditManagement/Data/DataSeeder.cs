using Microsoft.EntityFrameworkCore;
using PKFAuditManagement.Models;

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
                   // Seeding for QC6 Second Sub Form
                   new QC6FormObjective() { QC6FormObjectiveID = 6, QC6SubFormID = 2, ObjectiveNo = 1, Objective = "Objective – Acceptance Procedures" },
                   // Seeding for QC6 Third Sub Form
                   new QC6FormObjective() { QC6FormObjectiveID = 7, QC6SubFormID = 3, ObjectiveNo = 1, Objective = "Objective – Acceptance Procedures" }
            );

            modelBuilder.Entity<QC6FormTestDescription>().HasData(
                   // Seeding for QC6 First Sub Form Test Descriptions
                   new QC6FormTestDescription()
                   {
                       QC6FormTestDescriptionID = 1,
                       QC6FormObjectiveID = 1,
                       DescriptionNo = 1,
                       Description = "<p>1. Decide whether we can rely on the management's integrity and whether association with the client may damage the firm's reputation. Consider:</p><ul><li>discussion with the party who introduced the client;</li><li>knowledge gained through work done by other departments within the firm;</li><li>media and any other reports;</li><li>client's relationship with any regulatory authority; and</li><li>client's attitude to fairness in financial reporting.</li></ul>"
                   }
             );
        }
    }
}
