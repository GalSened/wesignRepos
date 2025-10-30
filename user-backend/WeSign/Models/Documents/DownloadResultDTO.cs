using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents
{
    public class DownloadDTO
    {

        public DownloadDTO()
        {
            Files = new List<FilesDTO>();
        }
        public List<FilesDTO> Files { get; set; }

    }
    public class FilesDTO
    {
        public string Ext{ get; set; }
        public string Data { get; set; }
        public string Name { get; internal set; }
        
    }
    public class FilesExtraInfoDTO : FilesDTO
    {
        public string TemplateId { get; set; }
    }
    public class DownloadExtraInfoDTO
    {
        public DownloadExtraInfoDTO()
        {
            Files = new List<FilesExtraInfoDTO>();
        }
        public List<FilesExtraInfoDTO> Files { get; set; }
    }
}
