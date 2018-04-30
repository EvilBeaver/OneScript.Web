﻿

#Использовать "model"

Процедура ПриНачалеРаботыСистемы()
	
	ИспользоватьКонсольЗаданий();

	ИспользоватьСтатическиеФайлы();
	ИспользоватьМаршруты();

	//будет выполнено один раз
	ФоновыеЗадания.ВыполнитьЗадание("ОбработчикиФоновых", "АсинхронноеЗадание", Новый Массив);

	//будет выполнено до тех пор пока не выполнится (даже если есть исключение)
	ФоновыеЗадания.ВыполнитьЗадание("ОбработчикиФоновых", "ОтсутствующееАсинхронноеЗадание", Новый Массив);

	//попытается выполниться один раз - если будет исключение, повтора не произойдет
	ФоновыеЗадания.ВыполнитьЗаданиеОднократно("ОбработчикиФоновых", "АсинхронноеЗаданиеВыбрасыающееИсключение", Новый Массив);

КонецПроцедуры
