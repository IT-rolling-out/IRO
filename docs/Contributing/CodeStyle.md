#Кодгайд проекта

###### Данных правил придерживаться обязательно!

### Базовые требования:

- Длинные имена переменных, чтоб код был самодокументируемым. С автодополнением это не проблема.

- Не писать модификаторы доступа по умолчанию (internal для классов и private для методов и полей);

- Ставить "_" для приватных полей вначале имени.

- Названия методов и свойств ВСЕГДА с большой.

- Параметры в методах всегда с маленькой и без "_".

- Во всех случаях использовать var вместо явного указания типа переменной. 
  Исключение: неочевидная или некорректная типизация. В случаях, когда явно надо указать один тип, а компилятор определяет другой, указываем тип явно.

- Использовать summary в комментариях, комментарии всегда перед кодом.

- Не использовать кортежи, dynamic, object, словари без необходимости. 

- Разрешено использовать _ в именах классов и методов в исключительном случае, когда это улучшает читаемость кода (например, когда нужно описать принадлежность класса к двум подсистемам: DatabaseAccountClaimsProvider_WebmoneyWrap).

- При переносе вызовов методов ставить отступ в размере 4-х пробелов.

Неправильно:

	myLongVariableName
					.Foo1()
					.Foo2();
Правильно:

	myLongVariableName
		.Foo1()
    	.Foo2();


### Форматирование:

- В изменённых файлах надо всегда делать чистку форматирования (Ctrl+K+D) и чистку юзингов (Ctrl+.+Enter).

- Добавляйте как минимум 1 пустую строку между любыми частями класса. Исключения только в случае объявления свойств и соответствующих им полей.

- Строки кода должны помещаться на ваш экран. Не помещается - переносите.

- Везде, где можно ставить скобки, необходимо их ставить. К примеру в if/else или циклах.

- После закрывающей скобки, если там есть код - необходимо ставить пробел.

Правильно:

	if 
	{
		//some code
	}

	var a = 1;

Неправильно:

	if
	{
		//some code
	}
	var a = 1; //Не поставлен пробел после закрывающей скобки


### Порядок описания содержимого класса:

Порядок на основании типа содержимого:

1. поля;
2. свойства;
3. события;
4. конструкторы;
5. методы.

Порядок исходя из доступности элементов класса:

1. public;
2. internal;
3. protected;
4. private.

**Исключение!** При написании больших кусков кода рекомендуется выделять части по логике использования в #region. В данном случае порядок описания содержимого определяется на уровне региона, а не класса.

### Именования:

В названиях используем:

* **PascalCasing**: Имена методов, классов, интерфейсов, нэймспэйсов (в том числе алиасов для нэймспэйсов), свойств, полей (public и protected).

* **camelCasing**: Имена аргументов функций, локальных переменных, полей (private).
Венгерская нотация и under_score запрещены.

### Работа с исключениями:

- Если в функции выбрасываются исключения, то их необходимо отразить в комментарии к ней в виде:

		/// <exception cref="System.Exception">Thrown when...</exception> 

- Если поймали исключение и после обработки его нужно пробросить дальше, то пишите
throw; вместо throw ex; чтобы сохранить изначальный стектрейс, если ситуация не требует обратного.

- Если есть необходимость отобразить внутреннее исключение и добавить описание к нему - используйте AggregateException.