/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System;

namespace wrnchAI.wrAPI
{
    /// <summary>
    /// Class representing a person state, including 2d, 3d, raw 3d, head and Id.
    /// Everything except Pose2d might be null and should be checked before use.
    /// </summary>
    public class Person : ICloneable
    {
        public int Id { get; set; }
        public Pose2D Pose2d { get; set; }
        public Pose3D Pose3D { get; set; }
        public Pose3D RawPose3D { get; set; }
        public PoseHead PoseHead { get; set; }
        public PoseFace PoseFace { get; set; }
        public bool IsMain { get; set; }
        public IdState IdState { get; set; }

        public object Clone()
        {
            Person newPerson = MemberwiseClone() as Person;
            newPerson.Pose2d = Pose2d != null ? Pose2d.Clone() as Pose2D : null;
            newPerson.Pose3D = (this.Pose3D != null ? Pose3D.Clone() as Pose3D : null);
            newPerson.RawPose3D = (this.RawPose3D != null ? RawPose3D.Clone() as Pose3D : null);
            newPerson.PoseHead = (this.PoseHead != null ? PoseHead.Clone() as PoseHead : null);
            newPerson.PoseFace = (this.PoseFace != null ? PoseFace.Clone() as PoseFace : null);

            return newPerson;
        }

    }
}
