using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket.Models;

public partial class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string Position { get; set; } = null!;

    [InverseProperty("Employee")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
