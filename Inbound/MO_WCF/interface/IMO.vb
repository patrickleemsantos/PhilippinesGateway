Imports System.ServiceModel

<ServiceContract(), XmlSerializerFormat()> _
Public Interface IMO

    <OperationContract(IsOneWay:=True)> _
    Sub PostSmartMO(ByVal body As SmartMOStr)

End Interface

