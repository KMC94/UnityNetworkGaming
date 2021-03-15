using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EventHandler(NetEventState state);

// 이벤트 정의
public enum NetEventType
{
    Connect = 0, // 접속이벤트
    Disconnect,  // 접속 종료 이벤트
    SendError,   // 송신오류
    ReceiveError,// 수신오류
}

// 이벤트 결과
public enum NetEventResult
{
    Failure = -1, // 실패
    Success = 0, // 성공
}

// 알림 이벤트의 상태
public class NetEventState
{
    public NetEventType type;     // 이벤트 타입
    public NetEventResult result; // 이벤트 결과
}
