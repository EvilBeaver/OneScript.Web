﻿
Процедура ПриЗагрузкеБиблиотеки(Путь, СтандартнаяОбработка, Отказ)
	
	ФайлМанифеста = Новый Файл(ОбъединитьПути(Путь, "lib.config"));
	
	Если ФайлМанифеста.Существует() Тогда
		СтандартнаяОбработка = Ложь;
		ОбработатьМанифест(ФайлМанифеста.ПолноеИмя, Путь, Отказ);
	Иначе
		ОбработатьСтруктуруКаталоговПоСоглашению(Путь, СтандартнаяОбработка, Отказ);
	КонецЕсли;
	
КонецПроцедуры

Процедура ОбработатьМанифест(Знач Файл, Знач Путь, Отказ)
	
	Чтение = Новый ЧтениеXML;
	Чтение.ОткрытьФайл(Файл);
	Чтение.ПерейтиКСодержимому();
	
	Если Чтение.ЛокальноеИмя <> "package-def" Тогда
		Отказ = Истина;
		Чтение.Закрыть();
		Возврат;
	КонецЕсли;
	
	Пока Чтение.Прочитать() Цикл
		
		Если Чтение.ТипУзла = ТипУзлаXML.Комментарий Тогда

			Продолжить;

		КонецЕсли;

		Если Чтение.ТипУзла = ТипУзлаXML.НачалоЭлемента Тогда
		
			Если Чтение.ЛокальноеИмя = "class" Тогда
				ФайлКласса = Новый Файл(Путь + "/" + Чтение.ЗначениеАтрибута("file"));
				Если ФайлКласса.Существует() и ФайлКласса.ЭтоФайл() Тогда
					Идентификатор = Чтение.ЗначениеАтрибута("name");
					Если Не ПустаяСтрока(Идентификатор) Тогда
						ДобавитьКласс(ФайлКласса.ПолноеИмя, Идентификатор);
					КонецЕсли;
				Иначе
					ВызватьИсключение "Не найден файл " + ФайлКласса.ПолноеИмя + ", указанный в манифесте";
				КонецЕсли;

				Чтение.Прочитать(); // в конец элемента
			КонецЕсли;

			Если Чтение.ЛокальноеИмя = "module" Тогда
				ФайлКласса = Новый Файл(Путь + "/" + Чтение.ЗначениеАтрибута("file"));
				Если ФайлКласса.Существует() и ФайлКласса.ЭтоФайл() Тогда
					Идентификатор = Чтение.ЗначениеАтрибута("name");
					Если Не ПустаяСтрока(Идентификатор) Тогда
						ДобавитьМодуль(ФайлКласса.ПолноеИмя, Идентификатор);
					КонецЕсли;
				Иначе
					ВызватьИсключение "Не найден файл " + ФайлКласса.ПолноеИмя + ", указанный в манифесте";
				КонецЕсли;

				Чтение.Прочитать(); // в конец элемента
			КонецЕсли;
			
		КонецЕсли;
		
	КонецЦикла;
	
	Чтение.Закрыть();
	
КонецПроцедуры

Процедура ОбработатьСтруктуруКаталоговПоСоглашению(Путь, СтандартнаяОбработка, Отказ)
	
	КаталогКлассов = Новый Файл(ОбъединитьПути(Путь, "Классы"));
	КаталогМодулей = Новый Файл(ОбъединитьПути(Путь, "Модули"));
	
	Если КаталогКлассов.Существует() Тогда
		Файлы = НайтиФайлы(КаталогКлассов.ПолноеИмя, "*.os");
		Для Каждого Файл Из Файлы Цикл
			СтандартнаяОбработка = Ложь;
			ДобавитьКласс(Файл.ПолноеИмя, Файл.ИмяБезРасширения);
		КонецЦикла;
	КонецЕсли;
	
	Если КаталогМодулей.Существует() Тогда
		Файлы = НайтиФайлы(КаталогМодулей.ПолноеИмя, "*.os");
		Для Каждого Файл Из Файлы Цикл
			СтандартнаяОбработка = Ложь;
			ДобавитьМодуль(Файл.ПолноеИмя, Файл.ИмяБезРасширения);
		КонецЦикла;
	КонецЕсли;
	
КонецПроцедуры
