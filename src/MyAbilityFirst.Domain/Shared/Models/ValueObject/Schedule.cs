using System;

namespace MyAbilityFirst.Domain
{
	public class Schedule
	{

		#region Properties

		public DateTime Start { get; private set; }
		public DateTime End { get; private set; }

		#endregion

		#region Ctor

		protected Schedule()
		{
			// required by EF
		}

		public Schedule(DateTime start, DateTime end)
		{
			if (start >= end)
				throw new ArgumentOutOfRangeException("start", "The DateTime value for 'start' must be less than 'end'.");

			this.Start = start;
			this.End = end;
		}

		#endregion

		#region Helpers

		public Schedule NewStart(DateTime start)
		{
			return new Schedule(start, this.End);
		}

		public Schedule NewEnd(DateTime end)
		{
			return new Schedule(this.Start, end);
		}

		public bool Overlaps(Schedule otherSchedule)
		{
			return this.Start < otherSchedule.End && this.End > otherSchedule.Start;
		}

		#endregion

	}
}