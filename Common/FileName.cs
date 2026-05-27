using net.yarukizero.vrchat.shizuku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.yarukizero.vrchat.shizuku {
	internal class DefinedVrcParameter : IStoredVrcParameter {
		private static readonly IEnumerable<(string Name, VrcType Type)> template = new (string, VrcType)[] {
			("IsLocal", VrcType.Bool),
			("PreviewMode", VrcType.Int),
			("Viseme", VrcType.Int),
			("Voice", VrcType.Float),
			("GestureLeft", VrcType.Int),
			("GestureRight", VrcType.Int),
			("GestureLeftWeight", VrcType.Float),
			("GestureRightWeight", VrcType.Float),
			("AngularY", VrcType.Float),
			("VelocityX", VrcType.Float),
			("VelocityY", VrcType.Float),
			("VelocityZ", VrcType.Float),
			("VelocityMagnitude", VrcType.Float),
			("Upright", VrcType.Float),
			("Grounded", VrcType.Bool),
			("Seated", VrcType.Bool),
			("AFK", VrcType.Bool),
			("TrackingType", VrcType.Int),
			("VRMode", VrcType.Int),
			("MuteSelf", VrcType.Bool),
			("InStation", VrcType.Bool),
			("Earmuffs", VrcType.Bool),
			("IsOnFriendsList", VrcType.Bool),
			("AvatarVersion", VrcType.Int),
			("IsAnimatorEnabled", VrcType.Bool),
		};

		public string Name { get; }

		public VrcType Type { get; }

		public bool IsWrite { get; } = false;

		internal DefinedVrcParameter(string name, VrcType type) {
			this.Name = name;
			this.Type = type;
		}

		public static IEnumerable<IStoredVrcParameter> GetDefinedVrcParams()
			=> template.Select(x => new DefinedVrcParameter(x.Name, x.Type))
				.ToList()
				.AsReadOnly();
	}
}



