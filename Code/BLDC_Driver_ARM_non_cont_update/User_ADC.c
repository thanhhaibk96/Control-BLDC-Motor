/* Includes ------------------------------------------------------------------*/
#include "User_ADC.h"
#include "filter.h"
/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/
static uint16_t ADC_ConvertedValue[20];

static union ADC_BytesValue
{
	float Value;
	
	struct
	{
		unsigned a1:8;
		unsigned a2:8;
		unsigned a3:8;
		unsigned a4:8;
	} byte;
} UADC_Temp_On_Chip, UADC_Temp_LM35, UADC_iMotor;

/* Private const/macros ------------------------------------------------------*/
/**
 * @defgroup Module Pin define
 * @{
 */
		/** 
		* @brief   <Name> Pin define 
		*/

/**
 * @}
 */

/* Private variables ---------------------------------------------------------*/

/* Private function prototypes -----------------------------------------------*/

/* Exported function body ----------------------------------------------------*/

/* Private functions body ----------------------------------------------------*/
void UADC_GPIO_Configure(void)
{
	GPIO_InitTypeDef GPIO_InitStruct;
	
	RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOA,ENABLE);
	
	GPIO_InitStruct.GPIO_Mode = GPIO_Mode_AIN;
	GPIO_InitStruct.GPIO_Pin = GPIO_Pin_5|GPIO_Pin_4;
	GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_Init(GPIOA,&GPIO_InitStruct);
}

void UADC_ADC_DMA_Configure(void)
{
	ADC_InitTypeDef ADC_InitStruct;
	DMA_InitTypeDef DMA_InitStruct;
	NVIC_InitTypeDef NVIC_InitStruct;
	
	RCC_ADCCLKConfig(RCC_PCLK2_Div4);
	RCC_AHBPeriphClockCmd(RCC_AHBPeriph_DMA1,ENABLE);
	RCC_APB2PeriphClockCmd(RCC_APB2Periph_ADC1,ENABLE);
	
	ADC_InitStruct.ADC_Mode = ADC_Mode_Independent;
	ADC_InitStruct.ADC_ScanConvMode = ENABLE;
	ADC_InitStruct.ADC_ContinuousConvMode = ENABLE;
	ADC_InitStruct.ADC_ExternalTrigConv = ADC_ExternalTrigConv_None;
	ADC_InitStruct.ADC_DataAlign = ADC_DataAlign_Right;
	ADC_InitStruct.ADC_NbrOfChannel = 4;
	ADC_Init(ADC1, &ADC_InitStruct);
	
	ADC_RegularChannelConfig(ADC1,ADC_Channel_16,1,ADC_SampleTime_41Cycles5);
	ADC_RegularChannelConfig(ADC1,ADC_Channel_17,4,ADC_SampleTime_41Cycles5);
	ADC_RegularChannelConfig(ADC1,ADC_Channel_5,2,ADC_SampleTime_41Cycles5);
	ADC_RegularChannelConfig(ADC1,ADC_Channel_4,3,ADC_SampleTime_41Cycles5);
	
	DMA_DeInit(DMA1_Channel1);
	DMA_InitStruct.DMA_PeripheralBaseAddr = (uint32_t)(&(ADC1 -> DR));
	DMA_InitStruct.DMA_MemoryBaseAddr = (uint32_t)&ADC_ConvertedValue;
	DMA_InitStruct.DMA_DIR = DMA_DIR_PeripheralSRC;
	DMA_InitStruct.DMA_BufferSize = 20;
	DMA_InitStruct.DMA_PeripheralInc = DMA_PeripheralInc_Disable;
	DMA_InitStruct.DMA_MemoryInc = DMA_MemoryInc_Enable;
	DMA_InitStruct.DMA_PeripheralDataSize = DMA_PeripheralDataSize_HalfWord;
	DMA_InitStruct.DMA_MemoryDataSize = DMA_MemoryDataSize_HalfWord;
	DMA_InitStruct.DMA_Mode = DMA_Mode_Circular;
	DMA_InitStruct.DMA_Priority = DMA_Priority_High;
	DMA_InitStruct.DMA_M2M = DMA_M2M_Disable;
	DMA_Init(DMA1_Channel1,&DMA_InitStruct);
	
	DMA_ITConfig(DMA1_Channel1,DMA_IT_TC,ENABLE);
	
	NVIC_InitStruct.NVIC_IRQChannel = DMA1_Channel1_IRQn;
	NVIC_InitStruct.NVIC_IRQChannelCmd = ENABLE;
	NVIC_InitStruct.NVIC_IRQChannelPreemptionPriority = 0x00;
	NVIC_InitStruct.NVIC_IRQChannelSubPriority = 0x02;
	NVIC_Init(&NVIC_InitStruct);
	
	ADC_TempSensorVrefintCmd(ENABLE);
	
	ADC_Cmd(ADC1,ENABLE);
	DMA_Cmd(DMA1_Channel1,ENABLE);	
	ADC_DMACmd(ADC1,ENABLE);
	
	/* Enable ADC1 reset calibration register */   
  ADC_ResetCalibration(ADC1);
  /* Check the end of ADC1 reset calibration register */
  while(ADC_GetResetCalibrationStatus(ADC1));

  /* Start ADC1 calibration */
  ADC_StartCalibration(ADC1);
  /* Check the end of ADC1 calibration */
  while(ADC_GetCalibrationStatus(ADC1));
	
	  /* Start ADC1 Software Conversion */ 
  ADC_SoftwareStartConvCmd(ADC1, ENABLE);
}

uint16_t UADC_GetAvrValue(UADC_Channel_TypeDef _channel)
{
	return (ADC_ConvertedValue[_channel] + ADC_ConvertedValue[_channel + 4] + ADC_ConvertedValue[_channel + 8] + ADC_ConvertedValue[_channel + 12] + ADC_ConvertedValue[_channel + 16])/5;
}

float UADC_GetValue(UADC_Channel_TypeDef _channel)
{
	float value = 0.0;
	if(_channel == Temp_On_Chip)
	{
		value = UADC_Temp_On_Chip.Value;
	}
	else if(_channel == Temp_LM35)
	{
		value = UADC_Temp_LM35.Value;
	}
	else if(_channel == iMotor)
	{
		value = UADC_iMotor.Value;
	}
	return value;
}

uint8_t UADC_GetBytesConverted(UADC_Channel_TypeDef _channel, uint8_t byte)
{
	uint8_t value = 0;
	switch (_channel)
	{
		case Temp_On_Chip:
		{			
			if(byte == 1) value = UADC_Temp_On_Chip.byte.a1;
			else if(byte == 2) value = UADC_Temp_On_Chip.byte.a2;
			else if(byte == 3) value = UADC_Temp_On_Chip.byte.a3;
			else if(byte == 4) value = UADC_Temp_On_Chip.byte.a4;
			break;
		}
		case Temp_LM35:
		{			
			if(byte == 1) value = UADC_Temp_LM35.byte.a1;
			else if(byte == 2) value = UADC_Temp_LM35.byte.a2;
			else if(byte == 3) value = UADC_Temp_LM35.byte.a3;
			else if(byte == 4) value = UADC_Temp_LM35.byte.a4;
			break;
		}
		case iMotor:
		{			
			if(byte == 1) value = UADC_iMotor.byte.a1;
			else if(byte == 2) value = UADC_iMotor.byte.a2;
			else if(byte == 3) value = UADC_iMotor.byte.a3;
			else if(byte == 4) value = UADC_iMotor.byte.a4;
			break;
		}
	}
	return value;
}
/******************************************************************************/
/* Peripherals Interrupt Handlers --------------------------------------------*/
/******************************************************************************/

void DMA1_Channel1_IRQHandler(void)
{
		if(DMA_GetITStatus(DMA1_IT_TC1))
		{
			UADC_Temp_On_Chip.Value = (1.43 - ((1.20 * 4095.0 / UADC_GetAvrValue(Vref)) / 4095.0 * UADC_GetAvrValue(Temp_On_Chip))) / 0.0043 + 25.0;
			UADC_Temp_LM35.Value = 3.3*100.0*(float)UADC_GetAvrValue(Temp_LM35)/4095.0;
			//G = 1 + R1/R2; R1 = 680 Kohm, R2 = 100 Ohm; I = Uout/R
			// I = Uin/G*0.01 = ADC*Vref/(G*4095*0.01) (A)
			UADC_iMotor.Value = (float)UADC_GetAvrValue(iMotor)*1000.0*3.3/(11.0*4095.0*0.01);
			//UADC_iMotor.Value = (float)UADC_GetAvrValue(iMotor);
			DMA_ClearITPendingBit(DMA1_IT_GL1);
		}
}
