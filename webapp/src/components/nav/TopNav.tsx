import {Button, Input, Link} from "@heroui/react";
import {AcademicCapIcon, MagnifyingGlassIcon} from "@heroicons/react/24/solid";
import ThemeToggle from "@/components/nav/ThemeToggle";

export default function TopNav() {
    return (
        <header className='p-2 w-full fixed top-0 z-50 border-b bg-surface'>
            <div className='flex px-10 mx-auto'>
                <div className='flex items-center gap-6'>
                    <Link href='/' className='flex items-cemter gap-3 max-h-16'>
                        <AcademicCapIcon className='size-10 text-secondary'/>
                        <h3 className='text-xl font-semibold uppercase'>
                            Overflow
                        </h3>
                    </Link>
                    <nav className='flex gap-3 my-2 text-md text-neutral-500'>
                        <Link href='/'>About</Link>
                        <Link href='/'>Products</Link>
                        <Link href='/'>Contact</Link>
                        <Link href='/'></Link>
                    </nav>
                </div>
                <div className="flex items-center gap-4">
                    <div className="relative ml-6">
                        <MagnifyingGlassIcon
                            className="pointer-events-none absolute left-3 top-1/2 size-5 -translate-y-1/2"
                        />

                        <Input
                            type="search"
                            placeholder="Search..."
                            className="pl-10"
                        />
                    </div>

                    <div className="flex basis-1/4 shrink-0 justify-end gap-3">
                        <ThemeToggle/>
                        <Button variant="secondary">Login</Button>
                        <Button variant="secondary">Register</Button>
                    </div>
                </div>
            </div>
        </header>
    );
}