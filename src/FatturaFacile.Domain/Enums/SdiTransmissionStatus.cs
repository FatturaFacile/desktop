namespace FatturaFacile.Domain.Enums;

public enum SdiTransmissionStatus
{
    Draft = 0,
    ReadyToSend = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Rejected = 5,
}