using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class FileLoggerBehavior : MonoBehaviour {
	private void Start() {
		new FileLogger();
	}
}

public class FileLogger : ILogHandler {
	private FileStream m_FileStream;
	private StreamWriter m_StreamWriter;
	private ILogHandler m_DefaultLogHandler = Debug.unityLogger.logHandler;

	public FileLogger() {

		string filePath = Application.persistentDataPath + $"/development_logs_{DateTime.Now.ToFileTime()}.txt";
		Debug.Log($"App logs will be available inside: {filePath}");

		m_FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		m_StreamWriter = new StreamWriter(m_FileStream);
		m_StreamWriter.AutoFlush = true;

		// Replace the default debug log handler
		Debug.unityLogger.logHandler = this;
		Debug.Log($"Started Logging at {DateTime.Now}");
	}

	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
		m_StreamWriter.WriteLine(String.Format(format, args));
		m_StreamWriter.Flush();
		m_DefaultLogHandler.LogFormat(logType, context, format, args);
	}

	public void LogException(Exception exception, UnityEngine.Object context) {
		m_DefaultLogHandler.LogException(exception, context);
	}
}
