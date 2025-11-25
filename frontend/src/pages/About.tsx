import { A } from "@solidjs/router";
import { TbDeviceDesktopAnalytics } from "solid-icons/tb";

export default function AboutPage() {
  return (
    <div class="mt-2 grow flex flex-row gap-x-10">
      <div class="text-lg">
        <h1 class="text-2xl font-bold text-center mb-4">
          Про розроблений додаток
        </h1>
        <span>
          Даний додаток розроблявся для кваліфікаційної роботи магістра на тему:
        </span>
        <strong>
          <br />
          "Дослідження ефективності алгоритмів розподілених обчислень для задач
          обробки сигналів та зображень".
        </strong>
        <br />
        <br />
        <h3 class="text-xl font-bold mb-1"> Можливості застосунку</h3>
        <span>
          Даний додаток дозволяє обробляти сигналі одного виміру (аудіо сигнали)
          формату <strong>WAV</strong> та підтримує такі операції:
        </span>
        <ul class="ml-3">
          <li>• Фільтр високих частот;</li>
          <li>• Фільтр низьких частот;</li>
          <li>• Band-pass фільтр;</li>
          <li>• Короткочасне перетворення Фур'є.</li>
        </ul>
        <span>
          Натисність{" "}
          <A
            href="/1d"
            class="text-amber-400 hover:text-amber-300 duration-150 font-bold"
          >
            сюди
          </A>
          , щоб перейти на сторінку обробки сигналів.
        </span>
        <br />
        <br />
        <h3 class="text-xl font-bold mb-1"> Обробка зображень</h3>
        Для обробки 2D-сигналів перейдіть на строінку{" "}
        <A
          class="text-amber-400 hover:text-amber-300 duration-150 font-bold"
          href="/2d"
        >
          "Обробка зображень"
        </A>
        . <br />
        На цій сторінці ви зможете обробляти зображення і виконувати над ними
        наступні операції:
        <ul class="ml-3">
          <li>• Розмиття Гауса</li>
          <li>• Фільтр Лапласа</li>
        </ul>
        Список морфлологічних операцій
        <ul class="ml-3">
          <li>• Дилатація</li>
          <li>• Ерозія</li>
        </ul>
        <br />
        <h3 class="text-xl font-bold mb-1">Дослідження ефективності</h3>
        Після кожної виконаної операції над сигналом, ви зможете побачити
        результат тестування
        <br />
        такі як час виконання та виділену пам'ять.
        <br />
        Деякі операції, наприклад морфологічні, дозволяють запускати
        <br />
        ітеровані експерименти та зберігати результат у файл формату
        <strong> JSON</strong>.
        <br />
        <br />
        <h3 class="text-xl font-bold mb-1">Моніторинг ресурсів</h3>У вас є
        можливість побачити навантаженість системи, для цього натисніть на
        значок, <br /> що розташований у провому верхньому кутку
        <TbDeviceDesktopAnalytics size={32} class="inline-block ml-2" />. Він
        виводить наступну інформацію:
        <ol class="ml-3">
          <li>1. Навантаженість процесора;</li>
          <li>2. Швидкість процесора та кількість запущених процесів;</li>
          <li>3. Використану пам'ять відеокарти;</li>
          <li>4. Виділену оперативну пам'ять.</li>
        </ol>
        <br />
        <h3 class="text-xl font-bold mb-1">Підтримка технологій</h3>
        Якщо ви хочете переконатися в тому, які технології паралельної обробки
        <br />
        даних підтримує ваша система, перейдіть на сторінку{" "}
        <A
          class="text-amber-400 hover:text-amber-300 duration-150 font-bold"
          href="/can-i-use"
        >
          "Підтримка технологій"
        </A>
        . Переконайтеся, що ваша система підтримує:
        <ol class="ml-3">
          <li>1. OpenCL;</li>
          <li>2. CUDA.</li>
        </ol>
        <br />
        <h3 class="text-xl font-bold mb-1">Серверна частина</h3>
        Всі операції над сигналами виконуються виключно на стороні бек-енду.
        <br />
        Переконайтеся, що він запущений перед експлуатацією додатку.
      </div>
      <div class="border-r-2 border-dashed border-black"></div>
      <div class="basis-1/2">
        <h1 class="text-2xl font-bold text-center mb-4">Про автора</h1>
        <div class="flex flex-row gap-x-4">
          <img
            class="rounded-full w-22 h-22 border-2 border-gray-400 outline-2 "
            src="/author.webp"
          />
          <div class="flex flex-col justify-center gap-y-4 py-2">
            <span class="text-xl font-bold">Михаць Богдан Ігорович</span>
            <span class="text-lg">Дата народженння: 03.03.2003</span>
          </div>
        </div>
        <div class="text-lg mt-2">
          Михаць. Б. І є студентом 2-го курсу магістратури
          <br />
          Спеціальності 121 (ІПЗ)
          <br />
          Ужгородського Націонгального Університету
          <br />
          <br />
          Попередньо отримав ступніть бакалавра на спеціальності{" "}
          <strong>123 (Комп'ютерна іженерія)</strong>.
        </div>
      </div>
    </div>
  );
}
