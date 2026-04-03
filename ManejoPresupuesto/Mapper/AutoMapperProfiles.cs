using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
        }
    }
}
