using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebArchive.Models
{
    [Serializable()]
    public class Project
    {
        private string key;

        public long Id { get; set; }

        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        [Required]
        [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Project name must have from 2 to 100 characters")]
        public string Name { get; set; }

        public string Version { get; set; }

        public string Category { get; set; }

        [Display(Name = "Short description")]
        [Required]
        [StringLength(100, MinimumLength = 10,
        ErrorMessage = "Short description must have from 10 to 100 characters")]
        public string ShortDesc { get; set; }

        [Display(Name = "Owner of project")]
        [Required]
        [RegularExpression(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", ErrorMessage = "Email must have valid form")]
        public string Owner { get; set; }

        [Display(Name = "Developers involved into project")]
        [RegularExpression(@"(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*,\s*|\s*$))*", ErrorMessage = "Email must have valid form")]
        public string Authors { get; set; }

        [Display(Name = "Website")]
        [Required]
        [RegularExpression(@"^(http|https)://.*$", ErrorMessage = "Link must start with http:// or https://")]
        public string WebAdress { get; set; }

        [Display(Name = "Repository")]
        [RegularExpression(@"^(http|https)://.*$", ErrorMessage = "Link must start with http:// or https://")]
        public string Repository { get; set; }

        [Display(Name = "Written In")]
        public string WrittenIn { get; set; }

        public string Userland { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [Required]
        [MinLength(10, ErrorMessage = "Project description must be longer")]
        public string Description { get; set; }

        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }

        public string PostCreator { get; set; }

        public DateTime PostTime { get; set; }

        public DateTime EditTime { get; set; }

        [Display(Name = "Project logo")]
        public byte[] Logo { get; set; }

        [Display(Name = "Overview image")]
        public byte[] ImageUI { get; set; }

        public string Keygen
        {
            get
            {
                if (key == null)
                {
                    try
                    {
                        key = Regex.Replace(Name.ToLower(), "[^a-z0-9,]", "-");
                    }
                    catch
                    {
                        key = "null";
                    }
                }
                return key;
            }
            set { key = value; }
        }

    }

    public enum Status
    {
        Offline,

        Online,

        [Description("Connection refused or actively blocked after 2nd try")]
        Refused,

        [Display(Name = "Refused (1st try)")]
        [Description("Connection refused or actively blocked after change status from Online")]
        FirstRefused
    }

}