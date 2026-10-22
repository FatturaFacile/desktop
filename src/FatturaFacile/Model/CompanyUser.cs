// <copyright file="CompanyUser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Model;

public class CompanyUser(int id, int parentId, string firstName, string lastName, string jobTtile, string groupName, string email, string phone)
{
    public int Id { get; set; } = id;
    public int ParentId { get; set; } = parentId;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public string JobTitle { get; set; } = jobTtile;
    public string GroupName { get; set; } = groupName;
    public string Email { get; set; } = email;
    public string Phone { get; set; } = phone;

    public string FullName { get { return FirstName + " " + LastName; } }
}