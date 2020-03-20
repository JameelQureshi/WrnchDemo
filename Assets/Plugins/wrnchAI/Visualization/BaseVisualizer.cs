/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Collections.Generic;
using UnityEngine;
using wrnchAI.wrAPI;
using wrnchAI.Core;

namespace wrnchAI.Visualization
{
    public abstract class BaseVisualizer : MonoBehaviour
    {
        public abstract void UpdatePersons(List<Person> personsToUpdate);
        public abstract void AddNewPerson(Person personToAdd, Color visualColor);
        public abstract void RemovePerson(Person personToRemove);
    }
}
