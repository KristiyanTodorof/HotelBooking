using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Domain.BaseModels
{
    public interface IIdentity<T>
    {
        public T Id { get; set; }
    }
}
