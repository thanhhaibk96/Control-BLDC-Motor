/* Includes ------------------------------------------------------------------*/
#include "User_CAN.h"
#include "User_PWM.h"
#include "stdbool.h"
#include "User_Algorithm.h"
#include "User_ADC.h"

/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/
//--------Define ID CANBUS for AUV--------//
static int _IDCANBUS_ARM_1 = 0x121;
static int _IDCANBUS_ARM_2 = 0x122;
static int _IDCANBUS_MASS_SHIFTER = 0x123;
static int _IDCANBUS_PISTOL = 0x124;
static int _IDCANBUS_THRUSTER = 0x125;
static int _IDCANBUS_EPC = 0x126;

uint8_t CAN_RxMessage[8];
uint8_t CAN_TxData[8] = {0,0,0,0,0,0,0,0};
uint8_t _checksum = 0;

/*---------------------------------Timing-------------------------------------*/

static struct
{
	bool CAN_StartComm;
	bool Control_Open_Loop;
	bool Close_Loop_PID;
	bool Close_Loop_Fuzzy;
	bool Motor_Run_CW;
	bool Motor_Run_CCW;
} Flag;
/* Private const/macros ------------------------------------------------------*/
#define CAN_BAUDRATE  1000      /* 1MBps   */
/* #define CAN_BAUDRATE  500*/  /* 500kBps */
 //#define CAN_BAUDRATE  250  /* 250kBps */
/* #define CAN_BAUDRATE  125*/  /* 125kBps */
/* #define CAN_BAUDRATE  100*/  /* 100kBps */ 
/* #define CAN_BAUDRATE  50*/   /* 50kBps  */ 
/* #define CAN_BAUDRATE  20*/   /* 20kBps  */ 
/* #define CAN_BAUDRATE  10*/   /* 10kBps  */ 

/* Private variables ---------------------------------------------------------*/

/* Private function prototypes -----------------------------------------------*/

/* Exported function body ----------------------------------------------------*/

/* Private functions body ----------------------------------------------------*/
void UCAN_GPIO_Configure(void)
{
	GPIO_InitTypeDef GPIO_InitStruct;
	
	RCC_APB2PeriphClockCmd(RCC_APB2Periph_AFIO|RCC_APB2Periph_GPIOA,ENABLE);
	// Configure CANRX
	GPIO_InitStruct.GPIO_Mode = GPIO_Mode_IPU;
	GPIO_InitStruct.GPIO_Pin = GPIO_Pin_11;
	GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_Init(GPIOA,&GPIO_InitStruct);
	
	// Configure CANTX
	GPIO_InitStruct.GPIO_Mode = GPIO_Mode_AF_PP;
	GPIO_InitStruct.GPIO_Pin = GPIO_Pin_12;
	GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_Init(GPIOA,&GPIO_InitStruct);
}

void UCAN_CAN_Configure(void)
{
	CAN_InitTypeDef CAN_InitStruct;
	CAN_FilterInitTypeDef CAN_FilterInitStruct;
	NVIC_InitTypeDef NVIC_InitStruct;
		
	uint16_t NVIC_Priority_Number;
	
	Flag.Control_Open_Loop = false;
	Flag.Close_Loop_PID = false;
	Flag.Close_Loop_Fuzzy = false;
	
	RCC_APB1PeriphClockCmd(RCC_APB1Periph_CAN1,ENABLE);
	// CAN_CLK = 32MHz
	CAN_DeInit(CAN1);
		
	CAN_StructInit(&CAN_InitStruct);
	CAN_InitStruct.CAN_TTCM = DISABLE;
  CAN_InitStruct.CAN_ABOM = DISABLE;
  CAN_InitStruct.CAN_AWUM = DISABLE;
  CAN_InitStruct.CAN_NART = DISABLE;
  CAN_InitStruct.CAN_RFLM = DISABLE;
  CAN_InitStruct.CAN_TXFP = ENABLE;
  CAN_InitStruct.CAN_Mode = CAN_Mode_Normal;
  CAN_InitStruct.CAN_SJW = CAN_SJW_1tq;  
  CAN_InitStruct.CAN_BS1 = CAN_BS1_3tq;
  CAN_InitStruct.CAN_BS2 = CAN_BS2_2tq;
	
	// BaudRate = CAN_CLK/(CAN_Prescaler*(1 + CAN_BS1 + CAN_BS2))
	#if CAN_BAUDRATE == 1000 /* 1MBps */
  CAN_InitStruct.CAN_Prescaler = 6;
	#elif CAN_BAUDRATE == 500 /* 500KBps */
  CAN_InitStruct.CAN_Prescaler = 12;
	#elif CAN_BAUDRATE == 250 /* 250KBps */
  CAN_InitStruct.CAN_Prescaler = 24;
	#elif CAN_BAUDRATE == 125 /* 125KBps */
  CAN_InitStruct.CAN_Prescaler = 48;
	#elif  CAN_BAUDRATE == 100 /* 100KBps */
  CAN_InitStruct.CAN_Prescaler = 60;
	#elif  CAN_BAUDRATE == 50 /* 50KBps */
  CAN_InitStruct.CAN_Prescaler = 120;
	#elif  CAN_BAUDRATE == 20 /* 20KBps */
  CAN_InitStruct.CAN_Prescaler = 300;
	#elif  CAN_BAUDRATE == 10 /* 10KBps */
  CAN_InitStruct.CAN_Prescaler = 600;
	#else
			#error "Please select first the CAN Baudrate in Private defines in User_CAN.c "
	#endif  /* CAN_BAUDRATE == 1000 */
	
	CAN_Init(CAN1,&CAN_InitStruct);
	
	CAN_FilterInitStruct.CAN_FilterNumber = 1;
  CAN_FilterInitStruct.CAN_FilterMode = CAN_FilterMode_IdList;
  CAN_FilterInitStruct.CAN_FilterScale = CAN_FilterScale_16bit;
  CAN_FilterInitStruct.CAN_FilterIdHigh = _IDCANBUS_MASS_SHIFTER << 5;
  CAN_FilterInitStruct.CAN_FilterIdLow = _IDCANBUS_MASS_SHIFTER << 5;
  CAN_FilterInitStruct.CAN_FilterMaskIdHigh = _IDCANBUS_MASS_SHIFTER << 5;
  CAN_FilterInitStruct.CAN_FilterMaskIdLow = _IDCANBUS_MASS_SHIFTER << 5;
  CAN_FilterInitStruct.CAN_FilterFIFOAssignment = 0;
  CAN_FilterInitStruct.CAN_FilterActivation = ENABLE;
  CAN_FilterInit(&CAN_FilterInitStruct);
	
	NVIC_Priority_Number = NVIC_EncodePriority(0,0,0);
	NVIC_SetPriority(USB_LP_CAN1_RX0_IRQn,NVIC_Priority_Number); 
	
	NVIC_PriorityGroupConfig(NVIC_PriorityGroup_1);
	
	NVIC_InitStruct.NVIC_IRQChannel = USB_LP_CAN1_RX0_IRQn;
	NVIC_InitStruct.NVIC_IRQChannelPreemptionPriority = 0x00;
	NVIC_InitStruct.NVIC_IRQChannelSubPriority = 0x05;
	NVIC_InitStruct.NVIC_IRQChannelCmd = ENABLE;
	NVIC_Init(&NVIC_InitStruct);
	
	CAN_ITConfig(CAN1,CAN_IT_FMP0,ENABLE);
}

void UCAN_Transmit(CAN_TypeDef* CANx, int _IDstd,int _length, uint8_t _data[])
{
	CanTxMsg _TxMessage;
	uint8_t mailbox;
	uint8_t 	status;
	_TxMessage.StdId = _IDstd; 
	_TxMessage.ExtId = 0x00;
	_TxMessage.RTR = CAN_RTR_DATA;
	_TxMessage.IDE = CAN_ID_STD;
	_TxMessage.DLC = _length;
			for(int j = 0;j < _length; j++)
			{
				_TxMessage.Data[j] = _data[j];
			}
	
	mailbox = CAN_Transmit(CANx,&_TxMessage);	

	//wait until CAN transmission is OK
	int32_t i = 0;
  while((status != CANTXOK) && (i != 0xFFFF))               
  {
    status = CAN_TransmitStatus(CANx,mailbox);
    i++;
  }
}

void UCAN_SystemOverLoad(void)
{
	CAN_TxData[0] = 'E';
	CAN_TxData[1] = 'R';
	CAN_TxData[2] = 'R';
	CAN_TxData[3] = 'I';
	CAN_TxData[4] = 'M';
	CAN_TxData[5] = 'O';
	CAN_TxData[6] = 'T';
	CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
	UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
}

void UCAN_StickedMotor(void)
{
	CAN_TxData[0] = 'E';
	CAN_TxData[1] = 'R';
	CAN_TxData[2] = 'R';
	CAN_TxData[3] = 'S';
	CAN_TxData[4] = 'T';
	CAN_TxData[5] = 'I';
	CAN_TxData[6] = 'C';
	CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
	UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
}

uint8_t UCAN_GetMessage(uint8_t bytes)
{
	return CAN_RxMessage[bytes];
}

bool UCAN_GetFlag(uint8_t type)
{
	bool value = false;
	if(type == Flag_Open_Loop)	value = Flag.Control_Open_Loop;
	else if(type == Flag_PID_Control) value = Flag.Close_Loop_PID;
	else if (type == Flag_Fuzzy_Control) value = Flag.Close_Loop_Fuzzy;
	else if(type == Flag_Motor_Run_CW) value = Flag.Motor_Run_CW;
	else if (type == Flag_Motor_Run_CCW) value = Flag.Motor_Run_CCW;
	return value;
}

void UCAN_ResetFlag(void)
{
	Flag.Control_Open_Loop = false;			
	Flag.Close_Loop_Fuzzy = false;
	Flag.Close_Loop_PID = false;
	Flag.Motor_Run_CW = false;
	Flag.Motor_Run_CCW = false;
}

uint8_t UCAN_Checksum(uint8_t *_data)
{
	uint8_t value = 0;

	//Calculate CheckSum (Byte)
	for (int i = 0; i < 7; i++)
	{
		value += _data[i];
	}
	value = ~value;
	value++;
	return (uint8_t)value;
}

/******************************************************************************/
/* Peripherals Interrupt Handlers --------------------------------------------*/
/******************************************************************************/

CanRxMsg RxMessage;
void USB_LP_CAN1_RX0_IRQHandler(void)
{
	uint8_t i = 0;
	CAN_Receive(CAN1,CAN_FIFO0,&RxMessage);
	_checksum = UCAN_Checksum(RxMessage.Data);
	if((RxMessage.StdId == _IDCANBUS_MASS_SHIFTER)&&(RxMessage.IDE == CAN_ID_STD)&&(RxMessage.DLC == 8)&&(UCAN_Checksum(RxMessage.Data) == RxMessage.Data[7]))
  {
		for(i = 0; i < 8; i++)
		{
			CAN_RxMessage[i] = RxMessage.Data[i];
		}
		//----------------------------------Start Communication----------------------------------//
		if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'A')&&(RxMessage.Data[2] == 'N')&&(RxMessage.Data[3] == 'O'))
		{
			Flag.CAN_StartComm = true;
		}
		else if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'A')&&(RxMessage.Data[2] == 'N')&&(RxMessage.Data[3] == 'C'))
		{
			Flag.CAN_StartComm = false;
		}
		//----------------------------------Controller----------------------------------//
		if(Flag.CAN_StartComm)
		{
			if((RxMessage.Data[0] == 'O')&&(RxMessage.Data[1] == 'L')&&(RxMessage.Data[2] == 'R'))
			{			
				Flag.Motor_Run_CW = true;
				Flag.Motor_Run_CCW = false;
				if((Flag.Close_Loop_PID == 0)&&(Flag.Close_Loop_Fuzzy == 0))
				{
					Flag.Control_Open_Loop = true;
					Flag.Close_Loop_Fuzzy = false;
					Flag.Close_Loop_PID = false;
					TIM_Cmd(TIM4,ENABLE);
					UHALL_ReadPosition(Motor_CW);
					UPWM_SetBytesDutyCycle(RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			if((RxMessage.Data[0] == 'O')&&(RxMessage.Data[1] == 'L')&&(RxMessage.Data[2] == 'L'))
			{			
				Flag.Motor_Run_CW = false;
				Flag.Motor_Run_CCW = true;
				if((Flag.Close_Loop_PID == 0)&&(Flag.Close_Loop_Fuzzy == 0))
				{
					Flag.Control_Open_Loop = true;
					Flag.Close_Loop_Fuzzy = false;
					Flag.Close_Loop_PID = false;
					TIM_Cmd(TIM4,ENABLE);
					UHALL_ReadPosition(Motor_CCW);
					UPWM_SetBytesDutyCycle(RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'P')&&(RxMessage.Data[2] == 'R'))
			{			
				Flag.Motor_Run_CW = true;
				Flag.Motor_Run_CCW = false;
				if((Flag.Control_Open_Loop == 0)&&(Flag.Close_Loop_Fuzzy == 0))
				{
					Flag.Close_Loop_PID = true;
					TIM_Cmd(TIM4,ENABLE);
					UALTHM_UpdateParameters(Update_SetSpeed,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'F')&&(RxMessage.Data[2] == 'R'))
			{
				Flag.Motor_Run_CW = true;
				Flag.Motor_Run_CCW = false;
				if((Flag.Control_Open_Loop == 0)&&(Flag.Close_Loop_PID == 0))
				{
					Flag.Close_Loop_Fuzzy = true;
					TIM_Cmd(TIM4,ENABLE);
					UALTHM_UpdateParameters(Update_SetSpeed,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			
			if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'P')&&(RxMessage.Data[2] == 'L'))
			{			
				Flag.Motor_Run_CW = false;
				Flag.Motor_Run_CCW = true;
				if((Flag.Control_Open_Loop == 0)&&(Flag.Close_Loop_Fuzzy == 0))
				{
					Flag.Close_Loop_PID = true;
					TIM_Cmd(TIM4,ENABLE);
					UALTHM_UpdateParameters(Update_SetSpeed,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			if((RxMessage.Data[0] == 'C')&&(RxMessage.Data[1] == 'F')&&(RxMessage.Data[2] == 'L'))
			{
				Flag.Motor_Run_CW = false;
				Flag.Motor_Run_CCW = true;
				if((Flag.Control_Open_Loop == 0)&&(Flag.Close_Loop_PID == 0))
				{
					Flag.Close_Loop_Fuzzy = true;
					TIM_Cmd(TIM4,ENABLE);
					UALTHM_UpdateParameters(Update_SetSpeed,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
				}
			}
			//----------------------------------Update Parameters----------------------------------//
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'K')&&(RxMessage.Data[2] == 'P'))
			{
				UALTHM_UpdateParameters(Update_Kp,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'K')&&(RxMessage.Data[2] == 'I'))
			{
				UALTHM_UpdateParameters(Update_Ki,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'K')&&(RxMessage.Data[2] == 'D'))
			{
				UALTHM_UpdateParameters(Update_Kd,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'G')&&(RxMessage.Data[2] == 'E'))
			{
				UALTHM_UpdateParameters(Update_Ge,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'D')&&(RxMessage.Data[2] == 'E'))
			{
				UALTHM_UpdateParameters(Update_Gde,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			if((RxMessage.Data[0] == 'G')&&(RxMessage.Data[1] == 'D')&&(RxMessage.Data[2] == 'U'))
			{
				UALTHM_UpdateParameters(Update_Gdu,RxMessage.Data[6], RxMessage.Data[5], RxMessage.Data[4], RxMessage.Data[3]);
			}
			
			//----------------------------------Request Parameters----------------------------------//
			if((RxMessage.Data[0] == 'R')&&(RxMessage.Data[1] == 'E')&&(RxMessage.Data[2] == 'Q')&&(RxMessage.Data[6] == 0x0A))
			{
				if((RxMessage.Data[3] == 'A')&&(RxMessage.Data[4] == 'L')&&(RxMessage.Data[5] == 'L'))
				{
					UHALL_GetActualSpeed();
					CAN_TxData[0] = 'R';
					CAN_TxData[1] = 'P';
					CAN_TxData[2] = 'V';
					CAN_TxData[3] = UHALL_GetBytesActualSpeed(1);
					CAN_TxData[4] = UHALL_GetBytesActualSpeed(2);
					CAN_TxData[5] = UHALL_GetBytesActualSpeed(3);
					CAN_TxData[6] = UHALL_GetBytesActualSpeed(4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
					UDELAY_us(10);
					
					CAN_TxData[0] = 'A';
					CAN_TxData[1] = 'T';
					CAN_TxData[2] = 'K';
					CAN_TxData[3] = UADC_GetBytesConverted(Temp_LM35,1);
					CAN_TxData[4] = UADC_GetBytesConverted(Temp_LM35,2);
					CAN_TxData[5] = UADC_GetBytesConverted(Temp_LM35,3);
					CAN_TxData[6] = UADC_GetBytesConverted(Temp_LM35,4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
					UDELAY_us(10);
					
					CAN_TxData[0] = 'A';
					CAN_TxData[1] = 'I';
					CAN_TxData[2] = 'M';
					CAN_TxData[3] = UADC_GetBytesConverted(iMotor,1);
					CAN_TxData[4] = UADC_GetBytesConverted(iMotor,2);
					CAN_TxData[5] = UADC_GetBytesConverted(iMotor,3);
					CAN_TxData[6] = UADC_GetBytesConverted(iMotor,4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
					UDELAY_us(10);
					
					CAN_TxData[0] = 'T';
					CAN_TxData[1] = 'D';
					CAN_TxData[2] = 'C';
					CAN_TxData[3] = UPWM_GetBytesDutyCycle(1);
					CAN_TxData[4] = UPWM_GetBytesDutyCycle(2);
					CAN_TxData[5] = UPWM_GetBytesDutyCycle(3);
					CAN_TxData[6] = UPWM_GetBytesDutyCycle(4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);	
					UDELAY_us(10);
				}
				
				if((RxMessage.Data[3] == 'R')&&(RxMessage.Data[4] == 'P')&&(RxMessage.Data[5] == 'V'))
				{
					UHALL_GetActualSpeed();
					CAN_TxData[0] = 'R';
					CAN_TxData[1] = 'P';
					CAN_TxData[2] = 'V';
					CAN_TxData[3] = UHALL_GetBytesActualSpeed(1);
					CAN_TxData[4] = UHALL_GetBytesActualSpeed(2);
					CAN_TxData[5] = UHALL_GetBytesActualSpeed(3);
					CAN_TxData[6] = UHALL_GetBytesActualSpeed(4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
				}
				
				if((RxMessage.Data[3] == 'A')&&(RxMessage.Data[4] == 'O')&&(RxMessage.Data[5] == 'C'))
				{
					CAN_TxData[0] = 'A';
					CAN_TxData[1] = 'O';
					CAN_TxData[2] = 'C';
					CAN_TxData[3] = UADC_GetBytesConverted(Temp_On_Chip,1);
					CAN_TxData[4] = UADC_GetBytesConverted(Temp_On_Chip,2);
					CAN_TxData[5] = UADC_GetBytesConverted(Temp_On_Chip,3);
					CAN_TxData[6] = UADC_GetBytesConverted(Temp_On_Chip,4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
				}
				
				if((RxMessage.Data[3] == 'A')&&(RxMessage.Data[4] == 'T')&&(RxMessage.Data[5] == 'K'))
				{
					CAN_TxData[0] = 'A';
					CAN_TxData[1] = 'T';
					CAN_TxData[2] = 'K';
					CAN_TxData[3] = UADC_GetBytesConverted(Temp_LM35,1);
					CAN_TxData[4] = UADC_GetBytesConverted(Temp_LM35,2);
					CAN_TxData[5] = UADC_GetBytesConverted(Temp_LM35,3);
					CAN_TxData[6] = UADC_GetBytesConverted(Temp_LM35,4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
				}
				
				if((RxMessage.Data[3] == 'A')&&(RxMessage.Data[4] == 'I')&&(RxMessage.Data[5] == 'M'))
				{
					CAN_TxData[0] = 'A';
					CAN_TxData[1] = 'I';
					CAN_TxData[2] = 'M';
					CAN_TxData[3] = UADC_GetBytesConverted(iMotor,1);
					CAN_TxData[4] = UADC_GetBytesConverted(iMotor,2);
					CAN_TxData[5] = UADC_GetBytesConverted(iMotor,3);
					CAN_TxData[6] = UADC_GetBytesConverted(iMotor,4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
				}
				
				if((RxMessage.Data[3] == 'T')&&(RxMessage.Data[4] == 'D')&&(RxMessage.Data[5] == 'C'))
				{
					CAN_TxData[0] = 'T';
					CAN_TxData[1] = 'D';
					CAN_TxData[2] = 'C';
					CAN_TxData[3] = UPWM_GetBytesDutyCycle(1);
					CAN_TxData[4] = UPWM_GetBytesDutyCycle(2);
					CAN_TxData[5] = UPWM_GetBytesDutyCycle(3);
					CAN_TxData[6] = UPWM_GetBytesDutyCycle(4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);	
				}
				
				if((RxMessage.Data[3] == 'R')&&(RxMessage.Data[4] == 'S')&&(RxMessage.Data[5] == 'P'))
				{
					CAN_TxData[0] = 'R';
					CAN_TxData[1] = 'S';
					CAN_TxData[2] = 'P';
					CAN_TxData[3] = UALTHM_GetBytesSetSpeed(1);
					CAN_TxData[4] = UALTHM_GetBytesSetSpeed(2);
					CAN_TxData[5] = UALTHM_GetBytesSetSpeed(3);
					CAN_TxData[6] = UALTHM_GetBytesSetSpeed(4);
					CAN_TxData[7] = UCAN_Checksum(CAN_TxData);
					UCAN_Transmit(CAN1,_IDCANBUS_ARM_1,8,CAN_TxData);
				}
			}
		}
	}
}
