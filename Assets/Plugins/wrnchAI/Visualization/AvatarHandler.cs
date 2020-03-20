/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Collections.Generic;
using UnityEngine;
using wrnchAI.Core;
using wrnchAI.wrAPI;

namespace wrnchAI.Visualization
{
    /// <summary>
    ///  Sample visualizer that displays an animated avatar for each person based on 3D pose estimation.
    /// </summary>
    public class AvatarHandler : BaseVisualizer
    {
        [SerializeField]
        private GameObject m_avatarPrefab;
        public GameObject AvatarPrefab { get { return m_avatarPrefab; } }

        private List<AnimationController> m_avatars = new List<AnimationController>();

        public void Init(GameObject avatarPrefab, bool multiPerson = true)
        {
            m_avatarPrefab = avatarPrefab;
        }

        /// <summary>
        /// Add new tracked person, this will spawn a new avatar.
        /// </summary>
        /// <param name="personToAdd"></param>
        /// <param name="visualColor"></param>
        public override void AddNewPerson(Person personToAdd, Color visualColor)
        {
            if (m_avatarPrefab)
            {
                GameObject go = Instantiate<GameObject>(m_avatarPrefab);
                var animController = go.GetComponent<AnimationController>();

                animController.Id = personToAdd.Id;
                animController.CharacterColor = visualColor;
                animController.Appear();
                m_avatars.Add(animController);
            }
            else
            {
                Debug.Log("Cannot add new person, AvatarHandler not Initialized");
            }
        }

        /// <summary>
        /// Remove a person and remove its avatar from the active ones. 
        /// The avatar should timeout and self destroy on its own.
        /// </summary>
        /// <param name="personToRemove"></param>
        public override void RemovePerson(Person personToRemove)
        {
            AnimationController animController = m_avatars.Find(x => x.Id == personToRemove.Id);
            if (animController != null)
                animController.StartDeleting();
            m_avatars.Remove(animController);
        }

        /// <summary>
        /// Update all avatars with a list of persons returned by the pose estimator
        /// </summary>
        /// <param name="persons">A list of persons</param>
        public override void UpdatePersons(List<Person> persons)
        {
            foreach (var wac in m_avatars)
            {
                wac.UpdateJoints(persons.Find(x => x.Id == wac.Id));
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < m_avatars.Count; i++)
            {
                m_avatars[i].StartDeleting();
            }
        }
    }
}