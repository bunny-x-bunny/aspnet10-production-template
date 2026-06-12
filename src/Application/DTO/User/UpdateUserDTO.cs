using Domain.Enum;
using Domain.Models.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Application.DTO.User {
  public class UpdateUserDTO : UserBasicDTO {
    //[Description("Permissions")]
    //public required ISet<Permission> Permissions { get; set; }

    public new T UpdateEntity<T>(T entity) where T : AppUser {
      entity.PhoneNumber = PhoneNumber;
      entity.FullName = FullName;
      return entity;
    }
  }
}
