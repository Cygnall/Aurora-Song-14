// SPDX-FileCopyrightText: 2024 Pierson Arnold <greyalphawolf7@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System;
using Robust.Shared.Prototypes;


namespace Content.Shared._Floof.Consent;


/// <summary>
/// TODO
/// </summary>
[Prototype]
public sealed partial class ConsentTogglePrototype : IPrototype, IComparable
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("category")]
    public string Category { get; private set; } = "";

    [DataField("priority")]
    public int priority { get; private set; } = 0;

    public int CompareTo(object? obj) { // Allow for granular sorting to make the menu display consistently and intuitively
        if (obj is not ConsentTogglePrototype other)
            return -1;

        var cat = this.Category.CompareTo(other.Category);
        if (cat != 0)
            return cat; // Categories are different, sort by category
        if (this.priority != other.priority)
            return this.priority - other.priority; // Priorities are different, sort by priority

        return this.ID.CompareTo(other.ID); // Category and priority are the same, sort by ID
    }

    [DataField]
    public bool DefaultValue { get; private set; }
}
