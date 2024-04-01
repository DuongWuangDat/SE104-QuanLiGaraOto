using System;
using System.Collections.Generic;

namespace QuanLiGaraOto.DTOs
{

    
    public class CustomerDTO
    {
        public CustomerDTO()
        {
            this.Receptions = new HashSet<ReceptionDTO>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public virtual ICollection<ReceptionDTO> Receptions { get; set; }
    }
}
