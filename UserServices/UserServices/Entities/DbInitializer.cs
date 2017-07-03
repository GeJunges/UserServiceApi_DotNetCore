namespace UserServices.Entities {
    public class DbInitializer {

        public static void Seed(AppDbContext context) {

            // Ensure the database has been created
            context.Database.EnsureCreated();
        }
    }
}
