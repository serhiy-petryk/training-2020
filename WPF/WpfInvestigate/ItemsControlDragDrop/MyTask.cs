using System.Collections.ObjectModel;

namespace ItemsControlDragDrop
{
    enum TaskDuration
	{
		Unknown,
		VeryShort,
		Short,
		Medium,
		Long,
		VeryLong
	}

	class MyTask
	{
		public static ObservableCollection<MyTask> CreateTasks()
		{
			ObservableCollection<MyTask> list = new ObservableCollection<MyTask>();

			list.Add(new MyTask(TaskDuration.VeryShort, "Take out trash", "The trash can is full again.", false));
			list.Add(new MyTask(TaskDuration.Short, "Clean the bathroom", "It's my turn to scrub the tub. :(", true));
			list.Add(new MyTask(TaskDuration.Medium, "Write CodeProject article", "Need to write a CP article about ListViewDragDropManager.", true));
			//list.Add( null ); // Test what happens when there is a null item in the ListView.
			list.Add(new MyTask(TaskDuration.VeryLong, "Learn 3D programming", "Learn how to create slick UIs using the WPF 3D APIs.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "1Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "2Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "3Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "4Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "5Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "6G6et in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "7Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "8Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "9Get in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "qGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "wGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "eGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "rGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "tGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "yGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "uGet in touch with Tom", "It's been a while since I've spoken with him.", false));
			list.Add(new MyTask(TaskDuration.Unknown, "iGet in touch with Tom", "It's been a while since I've spoken with him.", false));

			return list;
		}

        private static int cnt = 0;
        public MyTask(TaskDuration duration, string name, string description, bool finished)
        {
            Id = cnt++;
			this.Duration = duration;
			this.Name = name;
			this.Description = description;
			Finished = finished;
		}

        public int Id { get; }
		public bool Finished { get; set; }
        public TaskDuration Duration { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
