import {Button, Link} from "@heroui/react";

export default function NotFound() {
    return (
        <div className='h-full flex items-center justify-center'>
            <div className='text-center space-y-6'>
                <h1 className='text-5xl font-bold'>404 - Page not found</h1>
                <p className='text-lg text-base-content/80'>
                    Sorry, the page you are looking for does not exist!
                </p>
                <Link href='/'>
                    <Button>Go Home</Button>
                </Link>
            </div>
        </div>
    );
}