namespace Cluster.Network;
public enum PacketType
{
	TCPHandshake = 1,
	UDPHandshake = 2,

	TCPPing = 3,
	UDPPing = 4,

	Voice = 5,

	UserDataRequest = 6,
	UserDataResponse = 7,

	SuccessfullLogin = 8,

	RoomPreview = 9,
	RoomRequest = 10,
	RoomCreationRequest = 11,
	Room = 12,
	RoomPreviewRequest = 13,

	VoiceChatConnectionRequest = 14,
	VoiceChatConnectionResponse = 15,
	DisconnectFromVoiceChat = 16,
	UserJoinedVoiceChat = 17,
	UserLeftVoiceChat = 18,
}