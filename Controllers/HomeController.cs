using demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;



namespace demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private string _dir;

        /*public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }*/

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
            _dir = _env.ContentRootPath + "\\Files\\";
            
        }

        SqlConnection conn = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=TEST_DB;Integrated Security=True");
        public IActionResult Index(String msg)
        {
           
            SqlCommand comm = new SqlCommand("select * from dbo.FORM_DATA;", conn);

            /*conn.Open();
            comm.ExecuteNonQuery();
            conn.Close();*/

            SqlDataAdapter d = new SqlDataAdapter(comm);
            DataTable dt = new DataTable();
            d.Fill(dt);

            var ds = dt.DataSet;

            var asen = dt.AsEnumerable();

           /* foreach ( var asenit in asen)
            {
                Console.Write( asenit.ItemArray[0].ToString(), asenit.ItemArray[1]);
            }

            List<TempForm> tempForms = Context.customer.Take(10).ToList();
*/
            
            ViewBag.message = msg;
            return View(asen);
        }
        public IActionResult FormSubmit(FileForm fileForm)
        {
            int i = 0;
            fileForm.fileNames = "";
            if (fileForm.files != null)
            {
                foreach (var file in fileForm.files)
                {
                    String fileName = file != null ? file.FileName : String.Empty;

                    using (var fileStream = new FileStream(Path.Combine(_dir, fileName), FileMode.Create, FileAccess.Write))
                    {
                        file.CopyTo(fileStream);
                    }
                    fileForm.fileNames += fileName + ", ";
                    i++;
                }
                fileForm.fileNames = fileForm.fileNames.Trim().TrimEnd(',');
            }

            string saveStaff = "INSERT into FORM_DATA (first_name,last_name,email,file_names) VALUES (@fname, @lname ,@email,@filenames)";

            using (SqlCommand querySaveData = new SqlCommand(saveStaff))
            {
                querySaveData.Connection = conn;
                querySaveData.Parameters.Add("@fname", SqlDbType.NVarChar, 500).Value = fileForm.firstName;
                querySaveData.Parameters.Add("@lname", SqlDbType.NVarChar, 500).Value = fileForm.lastName;
                querySaveData.Parameters.Add("@email", SqlDbType.NVarChar, 200).Value =  fileForm.email;
                querySaveData.Parameters.Add("@fileNames", SqlDbType.NVarChar, 1000).Value =  fileForm.fileNames;

                conn.Open();

                querySaveData.ExecuteNonQuery();
                conn.Close();
            }


            String msg = string.Format("Name: {0} {1}{2}Email: {3}{4}{5} Files Uploaded Successfully", 
                                        fileForm.firstName,
                                        fileForm.lastName,
                                        Environment.NewLine,
                                        fileForm.email,
                                        Environment.NewLine,
                                        i) ;
            return RedirectToAction("Index", new { msg = msg });
        }

        [HttpGet]
        public IActionResult Edit(int id, string firstName,string lastName, string email)
        {
            FileForm fileForm = new FileForm();
            fileForm.firstName = firstName;
            fileForm.lastName = lastName;
            fileForm.email = email;

            return View(fileForm);


        }

        [HttpPost]
        public IActionResult Edit(FileForm fileForm, int id, IEnumerable<IFormFile> files)
        {
            fileForm.files = files;
            int i = 0;
            fileForm.fileNames = "";
            if (fileForm.files != null)
            {
                foreach (var file in fileForm.files)
                {
                    String fileName = file != null ? file.FileName : String.Empty;

                    using (var fileStream = new FileStream(Path.Combine(_dir, fileName), FileMode.Create, FileAccess.Write))
                    {
                        file.CopyTo(fileStream);
                    }
                    fileForm.fileNames += fileName + ", ";
                    i++;
                }
                fileForm.fileNames = fileForm.fileNames.Trim().TrimEnd(',');
            }

            string saveStaff = "Update FORM_DATA  set first_name = @fname " +
                                                     ",last_name = @lname" +
                                                     ",email = @email" +
                                                     ",file_names = (select STRING_AGG(value, ',') from (select value from string_split((select isnull(file_names,'') + ',' + isnull(@fileNames,'') from FORM_DATA where id = @id),',') where value != '' group by value) a) " +
                                                     " Where id = @id;";

            using (SqlCommand querySaveData = new SqlCommand(saveStaff))
            {
                querySaveData.Connection = conn;
                querySaveData.Parameters.Add("@fname", SqlDbType.NVarChar, 500).Value = fileForm.firstName;
                querySaveData.Parameters.Add("@lname", SqlDbType.NVarChar, 500).Value = fileForm.lastName;
                querySaveData.Parameters.Add("@email", SqlDbType.NVarChar, 200).Value = fileForm.email;
                if (fileForm.fileNames != null)
                {
                    querySaveData.Parameters.Add("@fileNames", SqlDbType.NVarChar, 1000).Value = fileForm.fileNames;
                }
                else
                {
                    querySaveData.Parameters.Add("@fileNames", SqlDbType.NVarChar, 1000).Value = DBNull.Value;
                }

                querySaveData.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();

                querySaveData.ExecuteNonQuery();
                conn.Close();
            }


            String msg = string.Format("Record Updated Successfully");
            return RedirectToAction("Index", new { msg = msg });
        }

        public IActionResult Delete(int id)
        {

            string saveStaff = "Delete from FORM_DATA   Where id = @id;";

            using (SqlCommand querySaveData = new SqlCommand(saveStaff))
            {
                querySaveData.Connection = conn;
                querySaveData.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conn.Open();

                querySaveData.ExecuteNonQuery();
                conn.Close();
            }


            String msg = string.Format("Record Deleted Successfully");
            return RedirectToAction("Index", new { msg = msg });
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}