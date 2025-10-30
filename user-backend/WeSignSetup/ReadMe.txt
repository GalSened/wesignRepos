If you like to update each section in installer separately,
you can run app with '--options' parameter

You must run WeSignSetup application as admin

While adding SSL to sites , you should set sites in following details:
main app port = "443"
management app port = "10443"


DB Windows integration  - connection string
Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=WeSign;TrustServerCertificate=True;Data Source=WeSign\SQLExpress

In order to create csv file with data from app.settings run with '--uninstall' parameter