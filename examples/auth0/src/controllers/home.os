
Функция Index() Экспорт
	Возврат Представление("Главная");
КонецФункции

Функция Login() Экспорт
	
	ТипАутентификации = "Auth0";
	МаршрутПереадресации = "/";

	Возврат Новый РезультатДействияВнешнийВызов(ТипАутентификации, МаршрутПереадресации);

КонецФункции


Функция ИмяФункции()
	
КонецФункции