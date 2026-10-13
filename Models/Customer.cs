using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket.Models;

public partial class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(30)]
    public string Phone { get; set; } = null!;

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
