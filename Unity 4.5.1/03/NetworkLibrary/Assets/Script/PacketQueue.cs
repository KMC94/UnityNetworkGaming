using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class PacketQueue
{	
	// 패킷 저장 정보.
	struct PacketInfo
	{
		public int	offset;
		public int 	size;
	};
	
	// MemoryStream 클래스는 데이터를 끊김 없이 추가할 수 있는 스트림 버퍼로 통신 데이터를 간단하게 다룰 수 있어 편리합니다
	/// <summary>
	/// 단, MemoryStream 클래스는 데이터의 끊김이 없으므로 패킷으로는 다룰 수 없습니다. 그래서 큐에 추가하는 패킷의 패킷 크기와 저장 장소를
	/// 나타내는 오프셋을 구조체(structure)로 만든 패킷정보를 별도로 관리합니다. 패킷 정보는 List 클래스로 관리하고 패킷이 추가되면 리스트의 맨끝에
	/// 패킷정보를 추가합니다. 큐에서 패킷을 추출할 때는 앞에서부터 가져옵니다. 패킷 정보를 앞에서부터 꺼내고 그 패킷크기만큼의 데이터를 MemoryStream에서 가져옵니다.
	/// 이 패킷 큐 프로그램은 다음과 같이 구현합니다.
	/// </summary>
	// 데이터를 보존할 버퍼
	private MemoryStream 		m_streamBuffer;
	
	// 패킷 정보 관리 리스트
	private List<PacketInfo>	m_offsetList;
	
	// 메모리 배치 오프셋
	private int					m_offset = 0;

	// 락 오브젝트
	private Object lockObj = new Object();
	
	//  생성자(여기서 초기화합니다).
	public PacketQueue()
	{
		m_streamBuffer = new MemoryStream();
		m_offsetList = new List<PacketInfo>();
	}
	
	// 큐를 추가합니다.
	public int Enqueue(byte[] data, int size)
	{
		PacketInfo	info = new PacketInfo();
	
		// 패킷 정보 작성
		info.offset = m_offset;
		info.size = size;
			
		lock (lockObj)
		{
			// 패킷 저장 정보를 보존.
			m_offsetList.Add(info);
			
			// 패킷 데이터를 보존.
			m_streamBuffer.Position = m_offset;
			m_streamBuffer.Write(data, 0, size);
			m_streamBuffer.Flush();
			m_offset += size;
		}
		
		return size;
	}
	
	// 큐를 꺼냅니다.
	public int Dequeue(ref byte[] buffer, int size) {

		if (m_offsetList.Count <= 0) {
			return -1;
		}

		int recvSize = 0;
		lock (lockObj)
		{	
			PacketInfo info = m_offsetList[0];
		
			// 버퍼로부터 해당하는 패킷 데이터를 가져옵니다.
			int dataSize = Math.Min(size, info.size);
			m_streamBuffer.Position = info.offset;
			recvSize = m_streamBuffer.Read(buffer, 0, dataSize);
			
			// 큐 데이터를 꺼냈으므로 선두 요소 삭제.
			if (recvSize > 0) {
				m_offsetList.RemoveAt(0);
			}
			
			// 모든 큐 데이터를 꺼냈을 때는 스트림을 클리어해서 메모리를 절약합니다.
			if (m_offsetList.Count == 0)
			{
				Clear();
				m_offset = 0;
			}
		}
		
		return recvSize;
	}

	// 큐를 클리어합니다.	
	public void Clear()
	{
		byte[] buffer = m_streamBuffer.GetBuffer();
		Array.Clear(buffer, 0, buffer.Length);
		
		m_streamBuffer.Position = 0;
		m_streamBuffer.SetLength(0);
	}
}

